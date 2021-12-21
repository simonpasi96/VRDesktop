using UnityEngine;
using UnityEngine.Events;

public class PosenetPointingSelectable : MonoBehaviour
{
    [SerializeField]
    UnityEvent Selected;
    Vector2 startSizeDelta;
    float highlightScaleFactor = 1.5f;
    public bool IsSelected { get; private set; }


    private void Awake()
    {
        startSizeDelta = ((RectTransform)transform).sizeDelta;
    }


    public void Select()
    {
        ((RectTransform)transform).sizeDelta = startSizeDelta * highlightScaleFactor;
        IsSelected = true;
        Selected.Invoke();
    }

    public void Deselect()
    {
        ((RectTransform)transform).sizeDelta = startSizeDelta;
        IsSelected = false;
    }
}