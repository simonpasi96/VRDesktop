using UnityEngine;

public class PosenetSwipeArrows_Simple : MonoBehaviour
{
    [SerializeField]
    PosenetSwipe posenetSwipe;
    [SerializeField]
    GameObject rightArrow, leftArrow;


    private void Reset()
    {
        posenetSwipe = FindObjectOfType<PosenetSwipe>();
        if (transform.childCount < 2)
            return;
        rightArrow = transform.GetChild(0).gameObject;
        leftArrow = transform.GetChild(1).gameObject;
    }

    private void Start()
    {
        if (!posenetSwipe)
            return;

        posenetSwipe.LeftStart += delegate { leftArrow.SetActive(true); };
        posenetSwipe.LeftEnd += delegate { leftArrow.SetActive(false); };
        posenetSwipe.RightStart += delegate { rightArrow.SetActive(true); };
        posenetSwipe.RightEnd += delegate { rightArrow.SetActive(false); };
        
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
    }
}