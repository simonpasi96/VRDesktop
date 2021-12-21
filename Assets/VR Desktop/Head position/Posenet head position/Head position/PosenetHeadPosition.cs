using UnityEngine;

/// <summary>
/// Tries to get the position of the head from the current Posenet.
/// </summary>
public class PosenetHeadPosition : MonoBehaviour
{
    PosenetPoseTracker PoseTracker { get { return PosenetPoseTracker.Instance; } }
    PosenetTrackedPose CurrentPose { get { return PoseTracker.Pose; } }
    [SerializeField]
    [Tooltip("The width of the head in real life (in meters).")]
    float headWidth = .14f;
    float headToEyesRatio;
    float verticalFov;

    public bool HasValidPosition { get; private set; }
    public Vector3 Position { get; private set; }
    public float FaceTextureRatio { get { return Vector2.Distance(CurrentPose.GetKeypoint(PoseKeypointType.leftEar).position, CurrentPose.GetKeypoint(PoseKeypointType.rightEar).position) / PosenetPoseTracker.Instance.RawResult.image.width; } }


    private void Awake()
    {
        GetVerticalFOV();
    }

    private void Start()
    {
        PoseTracker.PoseUpdated += UpdatePosition;
    }


    void UpdatePosition()
    {
        // Try to get a valid position.
        try
        {
            Position = (Vector3)HeadPositionFromPose(CurrentPose);
            HasValidPosition = true;
        }
        catch
        {
            HasValidPosition = false;
        }
    }

    Vector3? HeadPositionFromPose(PosenetTrackedPose pose)
    {
        // Get the pose keypoints that we need.
        PoseNetKeypoint lEar = pose.GetKeypoint(PoseKeypointType.leftEar);
        PoseNetKeypoint rEar = pose.GetKeypoint(PoseKeypointType.rightEar);
        PoseNetKeypoint lEye = pose.GetKeypoint(PoseKeypointType.leftEye);
        PoseNetKeypoint rEye = pose.GetKeypoint(PoseKeypointType.rightEye);
        if ((lEar == null && lEye == null) || (rEar == null && rEye == null))
            return null;

        // Get current distance from camera.
        float currentHeadWidth = Vector2.Distance((lEar == null ? lEye : lEar).position, (rEar == null ? rEye : rEar).position);
        float distanceFromCamera = GetDistanceFromOnScreenRatio(headWidth, Vector2.Distance(lEar.position, rEar.position) / PoseTracker.RawResult.image.width);

        // Get face position on texture (-1 to 1).
        Vector2 FacePosOnTexture = Utilities.Vector2Center(lEar.RelativePosition(), rEar.RelativePosition()) * 2;

        // Convert position to world units (using SOHCAHTOA with the right triangle created by the camera and the screen).
        float halfScreenWidth = Mathf.Tan(Mathf.Deg2Rad * (WebcamFOVInput.FOV * .5f)) * distanceFromCamera;
        float halfScreenHeight = Mathf.Tan(Mathf.Deg2Rad * (verticalFov * .5f)) * distanceFromCamera;
        float xPosition = FacePosOnTexture.x * halfScreenWidth;
        float yPosition = FacePosOnTexture.y * halfScreenHeight;

        // Return the position.
        return new Vector3(xPosition, yPosition, -distanceFromCamera);
    }

    float GetDistanceFromOnScreenRatio(float realWidth, float faceRectToTextureRatio)
    {
        // Get the width of the field of view where the head is in the real world.
        float screenLengthAtFace = realWidth * (1 / faceRectToTextureRatio);
        // Get the distance from the face length. (using SOHCAHTOA with the right triangle created by the camera and the face detection on the screen).
        return (screenLengthAtFace * .5f) / Mathf.Tan((WebcamFOVInput.FOV / 2) * Mathf.Deg2Rad);
    }

    void GetVerticalFOV()
    {
        float aspectRatioResult = ScreenSizeInput.AspectRatio.Height / (float)ScreenSizeInput.AspectRatio.Width;
        verticalFov = Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * (WebcamFOVInput.FOV * .5f)) * aspectRatioResult) * 2 * Mathf.Rad2Deg;
    }
}
