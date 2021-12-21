using UnityEngine;

/// <summary>
/// Triggers selection events if the head leans to the left or to the right.
/// </summary>
[RequireComponent(typeof(FaceDetectionAdvancedTransition))]
public class SelectionFromFace : MonoBehaviour
{
    public delegate void EventHandler();
    public static event EventHandler LeftSelect;
    public static event EventHandler RightSelect;

    FaceDetectionAdvancedTransition AdvancedFaceDetection { get { return (FaceDetectionAdvancedTransition)FaceDetection.Instance; } }
    float lastAngle;
    float CurrentRawAngle { get { return (float)AdvancedFaceDetection.CamShift.RotatedRect.angle; } }
    float CurrentAngle { get { return CurrentRawAngle > 90 ? CurrentRawAngle - 180 : CurrentRawAngle; } }
    [SerializeField]
    float threshold = 5;


    private void Start()
    {
        if (!AdvancedFaceDetection)
            return;
        AdvancedFaceDetection.SwitchingToCamshift += CheckForSelection;
    }


    private void CheckForSelection()
    {
        if (FaceTouchesSides())
            return;
        if (AdvancedFaceDetection.CamShift.RotatedRect.angle < 90)
            RightSelect?.Invoke();
        else
            LeftSelect?.Invoke();
    }

    
    bool FaceTouchesSides()
    {
        if (AdvancedFaceDetection.LastTouchesSidesGetter.Count == 0)
            return false;
        foreach (FaceDetectionAdvancedTransition.Side side in AdvancedFaceDetection.LastTouchesSidesGetter)
        {
            if (side == FaceDetectionAdvancedTransition.Side.Left || side == FaceDetectionAdvancedTransition.Side.Right)
                return true;
            // If we are touching the bottom side and the center of the face is close to the bottom, we are touching.
            if (side == FaceDetectionAdvancedTransition.Side.Down && AdvancedFaceDetection.CurrentFaceRect.center.y > AdvancedFaceDetection.CamShift.ResultRect.y + AdvancedFaceDetection.CamShift.ResultRect.height * .5f)
                return true;
            // If we are touching the top side and the center of the face is close to the top, we are touching.
            if (side == FaceDetectionAdvancedTransition.Side.Up && AdvancedFaceDetection.CurrentFaceRect.center.y < AdvancedFaceDetection.CamShift.ResultRect.y + AdvancedFaceDetection.CamShift.ResultRect.height * .5f)
                return true;
        }
        return false;
    }
}
