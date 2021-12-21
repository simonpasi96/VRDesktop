using UnityEngine;
using UnityEngine.UI;

public class HeadPositionDisplay : MonoBehaviour
{
    Vector2 detectionTextureSize;

    [SerializeField]
    Text xValue, yValue, zValue;


    void Update()
    {
        // Update position value displays.
        if (HeadPositionFromFace.Instance)
        {
            xValue.text = HeadPositionFromFace.Instance.Position.x.ToString("0.00");
            yValue.text = HeadPositionFromFace.Instance.Position.y.ToString("0.00");
            zValue.text = HeadPositionFromFace.Instance.Position.z.ToString("0.00");
        }
    }
}
