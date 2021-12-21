using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PosenetPointerLoadingCircle : MonoBehaviour
{
    Image image;


    private void Awake()
    {
        image = GetComponent<Image>();
        Hide();
    }

    private void Start()
    {
        PosenetPointer.Loading += UpdateFromPointer;
        PosenetPointer.NewHover += delegate { Hide(); } ;
        PosenetPointer.Selected += delegate { Hide(); };
        PosenetPointer.SelectionEnd += delegate { Hide(); };
    }


    void UpdateFromPointer(PosenetPointer pointer)
    {
        if (!image.enabled)
        {
            image.enabled = true;
            transform.position = pointer.CurrentSelectable.transform.position;
        }
        image.fillAmount = pointer.LoadingProgression;
    }

    void Hide()
    {
        image.enabled = false;
    }
}