using OpenCVForUnity.CoreModule;
using UnityEngine;

/// <summary>
/// Gets the head position relative to the camera.
/// </summary>
public class HeadPositionFromFace : MonoBehaviour
{
    Vector2 textureSize;

    float verticalFov;

    #region Face position -----------
    // Using an average face length (forehead to chin).
    public const float FaceLength = .218f;

    public bool FaceIsTracked { get { return FaceDetection.Instance.CurrentFaceRect != null; } }
    public Point DetectedFaceCenter { get { return FaceDetection.Instance.CurrentFaceRect.center; } }
    public float DetectedFaceWidth { get { return FaceDetection.Instance.CurrentFaceRect.width; } }

    /// <summary>
    /// Face position on texture, from -1 to 1.
    /// </summary>
    public Vector2 FacePosOnTexture { get; private set; }
    /// <summary>
    /// Head position in world space. Distance is based on the detected face's length.
    /// </summary>
    public Vector3 Position { get; private set; }

    public float FaceTextureRatio {
        get {
            if (FaceDetection.Instance.CurrentFaceRect == null || (textureSize.x * textureSize.y) == 0)
                return -1;
            return FaceDetection.Instance.CurrentFaceRect.Area() / (textureSize.x * textureSize.y);
        }
    }
    #endregion --------------

    #region Debug perspective increase --------
    [SerializeField]
    [Range(0, 1)]
    float debugPerspectiveIncrease = 0;
    float startHeadDistance;
    bool gotStartHeadDistance;
    #endregion --------------------------------

    public static HeadPositionFromFace Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
        GetVerticalFOV();
    }

    private void Start()
    {
        textureSize = FaceDetection.Instance.RequestedResolution;

        // Get start head distance for debug perspective increase.
        FaceDetection.Instance.CamShiftSent += delegate
        {
            startHeadDistance = Mathf.Abs(Position.z);
            gotStartHeadDistance = true;
        };
    }

    private void Update()
    {
        if (FaceIsTracked)
            UpdateFacePosition();
    }


    void UpdateFacePosition()
    {
        float distanceFromCamera = GetDistanceFromSize(FaceLength, DetectedFaceWidth / textureSize.x);

        // Get face position on texture (-1 to 1).
        FacePosOnTexture = (new Vector2((float)DetectedFaceCenter.x / textureSize.x, 1 - ((float)DetectedFaceCenter.y / textureSize.y)) - (Vector2.one * .5f)) * 2;
        // Convert position to world units (using SOHCAHTOA with the right triangle created by the camera and the screen).
        float halfScreenWidth = Mathf.Tan(Mathf.Deg2Rad * (WebcamFOVInput.FOV * .5f)) * distanceFromCamera;
        float halfScreenHeight = Mathf.Tan(Mathf.Deg2Rad * (verticalFov * .5f)) * distanceFromCamera;

        float xPosition = FacePosOnTexture.x * halfScreenWidth;
        float yPosition = FacePosOnTexture.y * halfScreenHeight;

        // Debug head distance forcing.
        if (gotStartHeadDistance)
            distanceFromCamera += (distanceFromCamera - startHeadDistance) * debugPerspectiveIncrease;

        Position = new Vector3(xPosition, yPosition, -distanceFromCamera);
    }

    float GetDistanceFromSize(float realWidth, float faceRectToTextureRatio)
    {
        float screenLengthAtFace = realWidth * (1 / faceRectToTextureRatio);
        // Using SOHCAHTOA with the right triangle created by the camera and the face detection on the screen.
        return (screenLengthAtFace * .5f) / Mathf.Tan((WebcamFOVInput.FOV / 2) * Mathf.Deg2Rad);
    }

    #region FOV ----
    public void OnFOVChange()
    {
        GetVerticalFOV();
    }

    void GetVerticalFOV()
    {
        float aspectRatioResult = ScreenSizeInput.AspectRatio.Height / (float)ScreenSizeInput.AspectRatio.Width;
        verticalFov = Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * (WebcamFOVInput.FOV * .5f)) * aspectRatioResult) * 2 * Mathf.Rad2Deg;
    }
    #endregion ------
}
