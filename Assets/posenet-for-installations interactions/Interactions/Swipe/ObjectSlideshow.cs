using System.Collections.Generic;
using UnityEngine;

public class ObjectSlideshow : MonoBehaviour
{
    [SerializeField]
    float transitionDuration = .5f;
    bool animationEnded = true;

    int currentChild;
    int previousChild = -1;

    [SerializeField]
    float translateStrength = 1;

    Dictionary<Transform, Renderer[]> renderersInChildren = new Dictionary<Transform, Renderer[]>();
    [SerializeField]
    Color emissionColor = Color.white;


    private void Start()
    {
        gameObject.HideChildren();
        transform.GetChild(0).gameObject.SetActive(true);

        // Get children renderers and setup emission.
        foreach (Transform child in transform)
        {
            Renderer[] rends = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in rends)
                foreach (Material mat in rend.materials)
                    mat.EnableKeyword("_EMISSION");
            renderersInChildren.Add(child, rends);
        }
    }


    public void Next()
    {
        if (!animationEnded)
        {
            // Stop the previous animation.
            StopAllCoroutines();
            transform.GetChild(previousChild).gameObject.SetActive(false);
            transform.GetChild(currentChild).gameObject.SetActive(true);
        }
        // Start and animation to show another child.
        previousChild = currentChild;
        Utilities.LimitedIncrement(ref currentChild, transform.childCount);
        StartAnimation(transform.GetChild(previousChild), transform.GetChild(currentChild), true);
    }

    public void Previous()
    {
        if (!animationEnded)
        {
            // Stop the previous animation.
            StopAllCoroutines();
            transform.GetChild(previousChild).gameObject.SetActive(false);
            transform.GetChild(currentChild).gameObject.SetActive(true);
        }
        // Start and animation to show another child.
        previousChild = currentChild;
        Utilities.LimitedDecrement(ref currentChild, transform.childCount);
        StartAnimation(transform.GetChild(previousChild), transform.GetChild(currentChild), false);
    }

    void StartAnimation(Transform previousChild, Transform nextChild, bool left)
    {
        animationEnded = false;
        Vector3 defaultPosition = previousChild.position;
        Vector3 prevChildTargetPos = defaultPosition + Vector3.right * (left ? -1 : 1) * translateStrength;
        Vector3 nextChildStartPos = defaultPosition - Vector3.right * (left ? -1 : 1) * translateStrength;

        this.ProgressionAnim(transitionDuration * .5f, delegate (float progression)
        {
            // Animate the color and position of the previous child.
            previousChild.transform.position = Vector3.Lerp(defaultPosition, prevChildTargetPos, progression);
            foreach (Renderer rend in renderersInChildren[previousChild])
                foreach (Material mat in rend.materials)
                    mat.SetColor("_EmissionColor", emissionColor * progression);
        }, delegate
        {
            // Show the next object and animate its color and position.
            previousChild.gameObject.SetActive(false);
            nextChild.gameObject.SetActive(true);
            this.ProgressionAnim(transitionDuration * .5f, delegate (float progression)
            {
                nextChild.transform.position = Vector3.Lerp(nextChildStartPos, defaultPosition, progression);
                foreach (Renderer rend in renderersInChildren[nextChild])
                    foreach (Material mat in rend.materials)
                        mat.SetColor("_EmissionColor", emissionColor * (1 - progression));
            }, delegate { animationEnded = true; });
        });
    }
}