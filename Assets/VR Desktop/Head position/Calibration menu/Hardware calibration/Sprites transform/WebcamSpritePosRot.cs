using UnityEngine;

public class WebcamSpritePosRot : MonoBehaviour
{
    [SerializeField]
    RectTransform screenSizeReference;

    float Ratio { get { return screenSizeReference.rect.height / ScreenSizeInput.WidthHeight.y; } }

    [Header("Position")]
    [SerializeField]
    PosAxisToUse XPosRemap = PosAxisToUse.X;
    [SerializeField]
    PosAxisToUse YPosRemap = PosAxisToUse.Y;
    [SerializeField]
    PosAxisToUse ZPosRemap = PosAxisToUse.None;

    [Header("Rotation")]
    [SerializeField]
    RotAxisToUse rotationAxis = RotAxisToUse.Z;

    enum PosAxisToUse { X, Y, None }
    enum RotAxisToUse { X, XInverted, Z, ZInverted, None }


    private void Awake()
    {
        UpdatePosition();
        UpdateRotation();
        WebcamPositionInput.Changed += UpdatePosition;
        ScreenSizeInput.DiagonalChanged += UpdatePosition;
        WebcamRotationInput.Changed += UpdateRotation;
    }


    void UpdatePosition()
    {
        // Set our position relative to the screen's size.
        Vector2 newPosition = new Vector2();
        switch (XPosRemap)
        {
            case PosAxisToUse.X:
                newPosition.x = WebcamPositionInput.X * Ratio;
                break;
            case PosAxisToUse.Y:
                newPosition.y = WebcamPositionInput.X * Ratio;
                break;
            default:
                break;
        }
        switch (YPosRemap)
        {
            case PosAxisToUse.X:
                newPosition.x = WebcamPositionInput.Y * Ratio;
                break;
            case PosAxisToUse.Y:
                newPosition.y = WebcamPositionInput.Y * Ratio;
                break;
            default:
                break;
        }
        switch (ZPosRemap)
        {
            case PosAxisToUse.X:
                newPosition.x = WebcamPositionInput.Z * Ratio;
                break;
            case PosAxisToUse.Y:
                newPosition.y = WebcamPositionInput.Z * Ratio;
                break;
            default:
                break;
        }
        transform.localPosition = newPosition;
    }

    void UpdateRotation()
    {
        Vector3 newRotation = transform.localEulerAngles;
        switch (rotationAxis)
        {
            case RotAxisToUse.X:
                newRotation = newRotation.SetX(WebcamRotationInput.Rotation);
                break;
            case RotAxisToUse.XInverted:
                newRotation = newRotation.SetX(-WebcamRotationInput.Rotation);
                break;
            case RotAxisToUse.Z:
                newRotation = newRotation.SetZ(WebcamRotationInput.Rotation);
                break;
            case RotAxisToUse.ZInverted:
                newRotation = newRotation.SetZ(-WebcamRotationInput.Rotation);
                break;
            default:
                break;
        }
        transform.localEulerAngles = newRotation;
    }
}