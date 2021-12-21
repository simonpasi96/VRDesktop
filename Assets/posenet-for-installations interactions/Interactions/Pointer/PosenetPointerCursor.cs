using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PosenetPointerCursor : MonoBehaviour
{
    [SerializeField]
    Image image;
    PosenetPointer lastPointer;
    Vector3 startPosition;
    Vector3 fadeInPosOffset = Vector2.down * .2f;
    float cantSelectAlpha = .7f;


    private void Reset()
    {
        image = GetComponentInChildren<Image>();
    }

    private void Awake()
    {
        image.enabled = false;
        PosenetPointer.SelectionStart += OnSelectionStart;
        PosenetPointer.SelectionEnd += OnSelectionEnd;
        PosenetPointer.NewHover += MoveToSelectable;
        PosenetPointer.Selected += SnapToSelectedPosition;
    }


    #region Start/end on selection start/end ----------------
    void OnSelectionStart(PosenetPointer pointer)
    {
        image.enabled = true;
        image.color = image.color.SetA(0);
        // Set position if we are not on the same pointer.
        transform.position = pointer.CurrentSelectable.transform.position + fadeInPosOffset;
        MoveToSelectable(pointer);
        lastPointer = pointer;
    }

    void OnSelectionEnd(PosenetPointer pointer)
    {
        // Translate the cursor down and hide it.
        FadeToPosition(transform.position + fadeInPosOffset, 0, delegate { image.enabled = false; });
    }
    #endregion --------------------------------

    #region Moving to a new position ----------------------
    void SnapToSelectedPosition(PosenetPointer pointer)
    {
        StartCoroutine(SnappingToPositionAfterFrame(pointer.CurrentSelectable.transform.position));
        image.color = image.color.SetA(cantSelectAlpha);
    }

    IEnumerator SnappingToPositionAfterFrame(Vector3 newPos)
    {
        yield return null;
        transform.position = newPos;
    }

    void MoveToSelectable(PosenetPointer pointer)
    {
        StopAllCoroutines();
        FadeToPosition(pointer.CurrentSelectable.transform.position, pointer.CurrentSelectable.IsSelected ? cantSelectAlpha : 1);
    }

    #region Fading to a position -----------------
    void FadeToPosition(Vector3 targetPos, float alpha, System.Action endAction)
    {
        StartCoroutine(FadingToPosition(targetPos, alpha, endAction));
    }
    void FadeToPosition(Vector3 targetPos, float alpha)
    {
        StartCoroutine(FadingToPosition(targetPos, alpha, delegate { }));
    }

    IEnumerator FadingToPosition(Vector3 targetPos, float alpha, System.Action endAction)
    {
        // With a delay of one frame in order for the selectable's rect to be resized.
        yield return null;
        // Animate position and size over time.
        Vector3 startPos = transform.position;
        float startA = image.color.a;
        this.ProgressionAnim(.2f, delegate (float progression)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, progression * progression);
            image.color = image.color.SetA(Mathf.Lerp(startA, alpha, progression));
        }, endAction);
    }
    #endregion ----------------------------------
    #endregion ------------------------------------------
}