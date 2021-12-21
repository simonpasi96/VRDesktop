using UnityEngine;

public class ProjectionScreen : MonoBehaviour
{
    [SerializeField]
    Transform screenPosition, screenScale;


    private void Start()
    {
        UpdateTransform();
        CalibrationMenu.Calibrated += UpdateTransform;
    }


    void UpdateTransform()
    {
        // Set scale from size input.
            screenScale.localScale = screenScale.localScale = new Vector3(ScreenSizeInput.WidthHeight.x * .01f, ScreenSizeInput.WidthHeight.y * .01f, 1);
    }
}
