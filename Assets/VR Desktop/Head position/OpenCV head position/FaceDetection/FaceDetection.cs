using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils.Helper;
using UnityEngine;

[RequireComponent(typeof(CascadeFaceDetection), typeof(CamShiftFaceDetection))]
public class FaceDetection : MonoBehaviour
{
    [SerializeField]
    protected CascadeFaceDetection cascadeFaceDetection;
    public CascadeFaceDetection CascadeFaceDetection { get { return cascadeFaceDetection; } }
    [SerializeField]
    protected CustomWebCamTextureToMatHelper webcamMatHelper;
    public CustomWebCamTextureToMatHelper WebcamMatHelperGetter { get { return webcamMatHelper; } }
    public Vector2 RequestedResolution { get { return new Vector2(webcamMatHelper.RequestedWidth, webcamMatHelper.RequestedHeight); } }
    [SerializeField]
    protected CamShiftFaceDetection camShift;
    public CamShiftFaceDetection CamShift { get { return camShift; } }

    protected bool ShouldUseCamShift { get { return !cascadeFaceDetection.DetectsAFace && isFaceSent; } }
    bool isFaceSent;
    public delegate void EventHandler();
    public event EventHandler CamShiftSent;
    public event EventHandler SwitchingToCamshift;
    public event EventHandler SwitchingToCascade;

    #region Result face rect -------------------------
    public virtual CustomRect CurrentFaceRect {
        get {
            if (ShouldUseCamShift)
                return new CustomRect(camShift.ResultRect);
            else if (cascadeFaceDetection.DetectsAFace)
                return new CustomRect(cascadeFaceDetection.Rects[0]);
            else
                return null;
        }
    }
    #endregion --------------------------------------

    public static FaceDetection Instance { get; private set; }


    private void Reset()
    {
        webcamMatHelper = GetComponent<CustomWebCamTextureToMatHelper>();
        cascadeFaceDetection = GetComponent<CascadeFaceDetection>();
        camShift = GetComponent<CamShiftFaceDetection>();
        // Only one face detection component.
        if (GetComponents<FaceDetection>().Length > 1)
        {
            FaceDetection[] faceDetections = GetComponents<FaceDetection>();
            for (int i = faceDetections.Length - 1; i >= 0; i--)
                if (faceDetections[i] != this)
                {
                    DestroyImmediate(faceDetections[i]);
                    Debug.Log("Removed a faceDetection in excess on " + name + ".");
                }
        }
    }

    protected virtual void Awake()
    {
        webcamMatHelper.OnInitialized.AddListener(cascadeFaceDetection.OnWebCamTextureToMatHelperInitialized);
        camShift.canRender = false;
        camShift.enabled = false;
        Instance = this;
    }

    protected virtual void Update()
    {
        if (!isFaceSent && cascadeFaceDetection.DetectsAFace)
            SendFaceToCamShift();

        // Siwtch detection methods if needed.
        if (ShouldUseCamShift != camShift.canRender)
        {
            if (ShouldUseCamShift)
                SwitchingToCamshift?.Invoke();
            else
                SwitchingToCascade?.Invoke();
        }
        camShift.canRender = ShouldUseCamShift;
    }


    /// <summary>
    /// Send the face tracked by openCVFaceDetection to camShift. This should be done when the head is facing the camera.
    /// </summary>
    public void SendFaceToCamShift()
    {
        if (CurrentFaceRect == null)
            return;

        float xOffset = CurrentFaceRect.width * .2f;
        float yOffset = CurrentFaceRect.height * .4f;

        // Send 4 points from the face that was detected.
        camShift.SetPoint(new Point(CurrentFaceRect.center.x - xOffset, CurrentFaceRect.center.y - yOffset));
        camShift.SetPoint(new Point(CurrentFaceRect.center.x + xOffset, CurrentFaceRect.center.y - yOffset));
        camShift.SetPoint(new Point(CurrentFaceRect.center.x - xOffset, CurrentFaceRect.center.y + yOffset));
        camShift.SetPoint(new Point(CurrentFaceRect.center.x + xOffset, CurrentFaceRect.center.y + yOffset));

        isFaceSent = true;
        camShift.enabled = true;

        CamShiftSent?.Invoke();
    }
}