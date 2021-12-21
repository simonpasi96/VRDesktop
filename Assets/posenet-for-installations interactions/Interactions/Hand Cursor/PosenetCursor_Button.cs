using UnityEngine;
using UnityEngine.UI;

public class PosenetCursor_Button : MonoBehaviour
{
    Button button;
    Image image;
    Color startColor;


    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        startColor = image.color;
    }


    public void Click()
    {
        print("Click");
        button.onClick.Invoke();
    }

    public void OnHoverStart()
    {
        image.color = startColor * .8f;
    }

    public void OnHoverEnd()
    {
        image.color = startColor;
    }
}