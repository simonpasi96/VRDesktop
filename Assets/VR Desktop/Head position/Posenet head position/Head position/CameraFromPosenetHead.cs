using UnityEngine;

/// <summary>
/// Gets a smooth camera movement from the detected head position. Can reset the camera when the head's been lost for a long time.
/// </summary>
[RequireComponent(typeof(PosenetHeadPosition))]
public class CameraFromPosenetHead : MonoBehaviour
{
    PosenetHeadPosition headPosGetter;

    [SerializeField]
    Transform camChild;

    [SerializeField]
    bool recenterWhenLost = true;
    public bool RecenterWhenLost { get { return recenterWhenLost; } set { recenterWhenLost = value; } }
    [Tooltip("The delay after which the camera will be recentered when we aren't tracking anything.")]
    [SerializeField]
    float recenterDelay = 2;
    float lastLostTime;
    Vector3 startCamPos;
    bool lost;
    
    [Header("SmoothDamp")]
    [SerializeField]
    [Tooltip("Smoothing the movement forward and back.")]
    float distanceSmoothTime = .5f;
    float distanceVelocity;
    [SerializeField]
    [Tooltip("Smoothing the movement left, right, up, and down.")]
    float XYSmoothTime = .2f;
    Vector2 XYVelocity;


    private void Reset()
    {
        // Find camera in children.
        if (GetComponentInChildren<Camera>())
            camChild = GetComponentInChildren<Camera>().transform;
    }

    private void Awake()
    {
        headPosGetter = GetComponent<PosenetHeadPosition>();
        startCamPos = camChild.localPosition;
    }

    private void Start()
    {
        MatchTransformFromCalibration();
        CalibrationMenu.Calibrated += MatchTransformFromCalibration;
    }

    private void Update()
    {
        // Update the position of the camera.
        if (!headPosGetter.HasValidPosition)
        {
            if (!recenterWhenLost)
                return;

            // If we've been lost for a long time, recenter the camera.
            if (!lost)
            {
                lastLostTime = Time.time;
                lost = true;
            }
            if (Time.time - lastLostTime > recenterDelay)
                camChild.localPosition = GetSmoothDampPosition(startCamPos);
        }
        else
        {
            // Follow the position of the head.
            camChild.localPosition = GetSmoothDampPosition(headPosGetter.Position);
            lost = false;
        }
    }


    Vector3 GetSmoothDampPosition(Vector3 targetPos)
    {
        float newDistance = Mathf.SmoothDamp(camChild.localPosition.z, targetPos.z, ref distanceVelocity, distanceSmoothTime);
        Vector2 newXYPos = Vector2.SmoothDamp(camChild.localPosition, targetPos, ref XYVelocity, XYSmoothTime);
        return new Vector3(newXYPos.x, newXYPos.y, newDistance);
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