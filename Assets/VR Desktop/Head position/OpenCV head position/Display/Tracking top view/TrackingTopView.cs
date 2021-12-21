using UnityEngine;
using UnityEngine.UI;

public class TrackingTopView : MonoBehaviour
{
    [SerializeField]
    int widthInMeters = 1;
    [SerializeField]
    RectTransform headSprite;
    Image grid;
    [SerializeField]
    RectTransform lengthBar;

    Rect ThisRect { get { return ((RectTransform)transform).rect; } }


    private void Awake()
    {
        grid = GetComponent<Image>();
        UpdateGrid();
    }

    private void Update()
    {
        // Update head sprite position.
        float width = ThisRect.width;
        Vector3 relativePosition = (HeadPositionFromFace.Instance.Position / (float)widthInMeters) * (width * .5f);

        headSprite.anchoredPosition = new Vector2(relativePosition.x, relativePosition.z);
    }


    void UpdateGrid()
    {
        float xScale = 10 / (float)widthInMeters;
        float heightWidthRatio = ThisRect.height / ThisRect.width;
        float yScale = xScale * heightWidthRatio;

        grid.material.SetTextureScale("_MainTex", new Vector2(xScale, Mathf.Round(yScale)));
        grid.material.SetTextureOffset("_MainTex", new Vector2(xScale * .5f, 0));

        // Update the length bar.
        lengthBar.sizeDelta = Vector2.one * (ThisRect.width / (widthInMeters * 10));
    }
}
