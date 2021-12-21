using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using UnityEngine.UI;

public class FaceDetectionDisplay : MonoBehaviour
{
    [SerializeField]
    Image videoRenderer;
    Texture2D texture;

    enum RectToShow { Result, CascadeAndCamshift }
    [SerializeField]
    RectToShow rectToShow;
    [SerializeField]
    Color resultRectColor = Color.red;
    [SerializeField]
    Color cascadeFaceColor = Color.red;
    [SerializeField]
    Color camshiftFaceColor = new Color(1, .5f, 0);

    [Space]
    [SerializeField]
    RectTransform crosshair;

    FaceDetection FaceDetection { get { return FaceDetection.Instance; } }
    bool FaceIsTracked { get { return FaceDetection && FaceDetection.CurrentFaceRect != null; } }

    bool initialized;


    private void Reset()
    {
        videoRenderer = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        if (!FaceDetection)
            return;
        if (FaceDetection.WebcamMatHelperGetter.GetMat() != null)
            OnWebCamMatInitialized();
        else
            FaceDetection.WebcamMatHelperGetter.OnInitialized.AddListener(OnWebCamMatInitialized);
    }

    private void Update()
    {
        if (!initialized)
            return;

        Mat rgbaMat = FaceDetection.WebcamMatHelperGetter.GetMat();

        // Draw face rects to our texture.
        if (FaceIsTracked)
            DrawCurrentRectsToMat(rgbaMat);

        Utils.fastMatToTexture2D(rgbaMat, texture);

        // Update the crosshair.
        if (crosshair)
        {
            crosshair.anchoredPosition = HeadPositionFromFace.Instance.FacePosOnTexture * .5f * new Vector2(videoRenderer.rectTransform.rect.width, videoRenderer.rectTransform.rect.height);
            crosshair.gameObject.SetActive(FaceIsTracked);
        }
    }

    void DrawCurrentRectsToMat(Mat mat)
    {
        switch (rectToShow)
        {
            case RectToShow.Result:
                // Draw only the result rectangle.
                Imgproc.rectangle(mat, FaceDetection.CurrentFaceRect.Min.ToPoint(), FaceDetection.CurrentFaceRect.Max.ToPoint(), resultRectColor.ToScalar(), 2);
                break;
            case RectToShow.CascadeAndCamshift:
                // Draw the cascade rectangle or the camshift rectangles.
                if (!FaceDetection.CamShift.canRender)
                    Imgproc.rectangle(mat, FaceDetection.CascadeFaceDetection.Rects[0].tl(), FaceDetection.CascadeFaceDetection.Rects[0].br(), cascadeFaceColor.ToScalar(), 2);
                else
                {
                    Imgproc.rectangle(mat, FaceDetection.CamShift.ResultRect.tl(), FaceDetection.CamShift.ResultRect.br(), camshiftFaceColor.ToScalar(), 2);
                    for (int i = 0; i < 4; i++)
                        Imgproc.line(mat, FaceDetection.CamShift.RotatedRectPoints[i], FaceDetection.CamShift.RotatedRectPoints[(i + 1) % 4], camshiftFaceColor.ToScalar(), 2);
                }
                break;
            default:
                break;
        }
    }

    void OnWebCamMatInitialized()
    {
        Mat webCamTextureMat = FaceDetection.WebcamMatHelperGetter.GetMat();
        texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        videoRenderer.material.mainTexture = texture;
        initialized = true;
    }
}
