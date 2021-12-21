using UnityEngine;
using UnityEngine.UI;

public class ItemCarouselSelection : MonoBehaviour
{
    [SerializeField]
    Button buttonNext, buttonPrevious;
    [SerializeField]
    ItemCarousel carousel;
    [SerializeField]
    [Tooltip("Use the rotation of the head to move the carousel.")]
    bool useHeadSelection = true;


    private void Reset()
    {
        carousel = GetComponent<ItemCarousel>();
        carousel = GetComponentInParent<ItemCarousel>();
    }

    private void Awake()
    {
        buttonNext.onClick.AddListener(carousel.Next);
        buttonPrevious.onClick.AddListener(carousel.Previous);
        if (useHeadSelection)
        {
            SelectionFromFace.RightSelect += carousel.Next;
            SelectionFromFace.LeftSelect += carousel.Previous;
        }
    }
}
