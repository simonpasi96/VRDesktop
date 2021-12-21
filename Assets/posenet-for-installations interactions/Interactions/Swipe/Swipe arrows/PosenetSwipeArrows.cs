using System;
using System.Collections;
using UnityEngine;

public class PosenetSwipeArrows : MonoBehaviour
{
    [SerializeField]
    PosenetSwipe swiper;

    [SerializeField]
    Transform leftArrow, rightArrow;
    Renderer leftRend, rightRend;

    IEnumerator animatingLeft;
    IEnumerator animatingRight;

    /// <summary> The alpha at which the arrow is supposed when not animated. </summary>
    float LAlphaOutsideAnim { get { return swiper.IsLeftReady ? 1 : .3f; } }
    float RAlphaOutsideAnim { get { return swiper.IsRightReady ? 1 : .3f; } }


    private void Reset()
    {
        swiper = FindObjectOfType<PosenetSwipe>();
        if (transform.childCount > 1)
        {
            leftArrow = transform.GetChild(0);
            rightArrow = transform.GetChild(1);
        }
    }

    private void Start()
    {
        leftRend = leftArrow.GetComponentInChildren<Renderer>(true);
        rightRend = rightArrow.GetComponentInChildren<Renderer>(true);

        swiper.SwipedLeft.AddListener(delegate { AnimateArrow(leftArrow, leftRend, animatingLeft, () => LAlphaOutsideAnim); });
        swiper.SwipedRight.AddListener(delegate { AnimateArrow(rightArrow, rightRend, animatingRight, () => RAlphaOutsideAnim); });

        swiper.LeftStart += delegate { leftArrow.gameObject.SetActive(true); };
        swiper.LeftEnd += delegate { leftArrow.gameObject.SetActive(false); };
        swiper.RightStart += delegate { rightArrow.gameObject.SetActive(true); };
        swiper.RightEnd += delegate { rightArrow.gameObject.SetActive(false); };
    }

    private void Update()
    {
        // Update the arrow's transparency when they aren't moving.
        if (!animatingLeft.IsRunning())
        {
            if (!swiper.CanSwipeLeft)
                leftArrow.gameObject.SetActive(false);
            else
            {
                leftArrow.gameObject.SetActive(true);
                leftRend.material.color = leftRend.material.color.SetA(LAlphaOutsideAnim);
            }
        }
        if (!animatingRight.IsRunning())
        {
            if (!swiper.CanSwipeRight)
                rightArrow.gameObject.SetActive(false);
            else
            {
                rightArrow.gameObject.SetActive(true);
                rightRend.material.color = rightRend.material.color.SetA(RAlphaOutsideAnim);
            }
        }
    }


    void AnimateArrow(Transform arrow, Renderer arrowRend, IEnumerator animationRoutine, Func<float> EndingAlpha)
    {
        // Animate the arrow's position and transparency over time.
        if (animationRoutine.IsRunning())
            StopCoroutine(animationRoutine);
        Vector3 startPosition = arrow.position;
        Vector3 startScale = arrow.localScale;
        animationRoutine = this.ProgressionAnim(.3f, delegate (float progression)
        {
            // Animate forward.
            float smoothProg = AniMath.SmoothStartEnd(progression);
            arrow.position = Vector3.Lerp(startPosition, startPosition + arrow.forward * .3f, smoothProg);
            arrow.localScale = Vector3.Lerp(startScale, startScale - Vector3.one * .2f, smoothProg);
            arrowRend.material.color = arrowRend.material.color.SetA(1 - smoothProg);
        }, delegate
        {
            // Reset position and animate alpha.
            arrow.position = startPosition;
            arrow.localScale = startScale;
            animationRoutine = this.ProgressionAnim(.3f, delegate (float progression)
            {
                arrowRend.material.color = arrowRend.material.color.SetA(progression * progression * EndingAlpha());
            }, delegate
            {
                animationRoutine = null;
            });
        });
    }
}