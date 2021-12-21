using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using UnityEngine.UI;

public class WebcamToImage : MonoBehaviour
{
    [SerializeField]
    RawImage rawImage;


    private void Reset()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Start()
    {
        if (FaceDetection.Instance.WebcamMatHelperGetter.GetMat() != null)
            OnWebCamTextureToMatHelperInitialized();
        else
            FaceDetection.Instance.WebcamMatHelperGetter.OnInitialized.AddListener(OnWebCamTextureToMatHelperInitialized);
    }

    private void Update()
    {
        Mat rgbaMat = FaceDetection.Instance.WebcamMatHelperGetter.GetMat();
        Utils.fastMatToTexture2D(rgbaMat, (Texture2D)rawImage.texture);
    }


    private void OnWebCamTextureToMatHelperInitialized()
    {
        Mat webCamTextureMat = FaceDetection.Instance.WebcamMatHelperGetter.GetMat();
        rawImage.texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
    }
}
