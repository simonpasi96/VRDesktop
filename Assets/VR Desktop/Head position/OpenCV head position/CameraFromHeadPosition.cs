using UnityEngine;

public class CameraFromHeadPosition : MonoBehaviour
{
    [SerializeField]
    Transform camChild;
    [Tooltip("The maximum world position on the z axis. If it is too small, things start to get weird.")]
    [SerializeField]
    float minOffset = -.1f;
    [SerializeField]
    Transform projectionScreen;

    [Header("Experimental smoothing")]
    [SerializeField]
    bool useExperimentalSmoothing = true;
    [SerializeField]
    Vector2 minMaxHeadVelocityToLerp = new Vector2(.003f, .2f);
    [SerializeField]
    Vector2 minMaxLerp = new Vector2(.1f, 1);
    [SerializeField]
    float powOnLerp = 1.5f;
    [SerializeField]
    [Tooltip("The smaller it is, the smaller the max head velocity is, in relationship to the face-texture ratio.")]
    float valueForRatioSmoothing = 0.01f;

    [Header("Standard smoothing")]
    [SerializeField]
    float constantLerp = .3f;

    public static CameraFromHeadPosition Instance { get; private set; }


    private void Reset()
    {
        if (GetComponentInChildren<ProjectionMatrix>() != null)
            projectionScreen = GetComponentInChildren<ProjectionMatrix>().projectionScreen.transform;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MatchTransformFromCalibration();
        CalibrationMenu.Calibrated += MatchTransformFromCalibration;
        // Set rotation from webcam rotation. 
        transform.eulerAngles = Vector3.right * WebcamRotationInput.Rotation;
        transform.localPosition += WebcamPositionInput.Position * .01f;
    }
    
    void Update()
    {
        if (!HeadPositionFromFace.Instance || !HeadPositionFromFace.Instance.FaceIsTracked || HeadPositionFromFace.Instance.FaceTextureRatio == 0)
            return;

        camChild.localPosition = Vector3.Lerp(camChild.localPosition, HeadPositionFromFace.Instance.Position, GetLerp());

        // Clamp position.
        if (camChild.localPosition.z > minOffset)
            camChild.localPosition = camChild.localPosition.SetZ(minOffset);
    }


    float GetLerp()
    {
        // Compensated max head velocity.
        float headVeloMaxCompensated = valueForRatioSmoothing / HeadPositionFromFace.Instance.FaceTextureRatio;

        // Smoothed movement.
        float lerp;
        if (useExperimentalSmoothing)
        {
            // Use minimum lerp when the head is not moving much, and maximum lerp when the head is moving a lot.
            lerp = (camChild.localPosition - HeadPositionFromFace.Instance.Position).magnitude.Remap(minMaxHeadVelocityToLerp.x, minMaxHeadVelocityToLerp.y, minMaxLerp.x, minMaxLerp.y);
            if (lerp > 0)
                lerp = Mathf.Pow(lerp, powOnLerp);
        }
        else
            lerp = constantLerp;

        return lerp;
    }

    /// <summary>
    /// Changes our position and rotation to match the position and rotation of the webcam in the calibration menu.
    /// </summary>
    void MatchTransformFromCalibration()
    {
        transform.localPosition += WebcamPositionInput.Position * .01f;
        transform.eulerAngles = Vector3.right * WebcamRotationInput.Rotation;
    }
}
