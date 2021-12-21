using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.VideoModule;
using System.Collections.Generic;
using UnityEngine;
using Rect = OpenCVForUnity.CoreModule.Rect;

/// <summary>
/// Adapted from CamShiftExample.
/// </summary>
[RequireComponent(typeof(CustomWebCamTextureToMatHelper), typeof(CascadeFaceDetection))]
public class CamShiftFaceDetection : MonoBehaviour
{
    bool shouldInit = true;

    Point storedTouchPoint;
    List<Point> roiPointList = new List<Point>();

    Mat hsvMat;
    Mat roiHistMat;

    bool shouldStartCamShift = false;
    TermCriteria termination = new TermCriteria(TermCriteria.EPS | TermCriteria.COUNT, 10, 1);

    CascadeFaceDetection faceDetection;

    [HideInInspector]
    public bool canRender = false;

    public Rect ResultRect { get; private set; }
    public RotatedRect RotatedRect { get; private set; }
    public Point[] RotatedRectPoints { get; private set; }


    void Update()
    {
        if (faceDetection && faceDetection.WebCamTextureToMatHelper.IsPlaying() && faceDetection.WebCamTextureToMatHelper.DidUpdateThisFrame())
            UpdateDetection();
    }


    void Init()
    {
        faceDetection = GetComponent<CascadeFaceDetection>();

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            faceDetection.WebCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif

        Mat webCamTextureMat = faceDetection.WebCamTextureToMatHelper.GetMat();
        hsvMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);

        shouldInit = false;
    }

    public void SetPoint(Point newPoint)
    {
        storedTouchPoint = newPoint;
        UpdateDetection();
    }

    void UpdateDetection()
    {
        if (shouldInit)
            Init();

        Mat rgbaMat = faceDetection.WebCamTextureToMatHelper.GetMat();
        Imgproc.cvtColor(rgbaMat, hsvMat, Imgproc.COLOR_RGBA2RGB);
        Imgproc.cvtColor(hsvMat, hsvMat, Imgproc.COLOR_RGB2HSV);

        if (storedTouchPoint != null)
        {
            SetNewTouchPoint(rgbaMat, storedTouchPoint);
            storedTouchPoint = null;
        }

        Point[] roiPoints = roiPointList.ToArray();
        if (shouldStartCamShift)
            StartCamShift(roiPoints);
        else if (roiPoints.Length == 4)
            UpdateCamShift(roiPoints);
    }

    private void StartCamShift(Point[] roiPoints)
    {
        shouldStartCamShift = false;

        // Start cam shift.
        using (MatOfPoint roiPointMat = new MatOfPoint(roiPoints))
            ResultRect = Imgproc.boundingRect(roiPointMat);

        if (roiHistMat != null)
        {
            roiHistMat.Dispose();
            roiHistMat = null;
        }
        roiHistMat = new Mat();

        using (Mat roiHSVMat = new Mat(hsvMat, ResultRect))
        using (Mat maskMat = new Mat())
        {
            Imgproc.calcHist(new List<Mat>(new Mat[] { roiHSVMat }), new MatOfInt(0), maskMat, roiHistMat, new MatOfInt(16), new MatOfFloat(0, 180));
            Core.normalize(roiHistMat, roiHistMat, 0, 255, Core.NORM_MINMAX);
        }
    }

    private void UpdateCamShift(Point[] roiPoints)
    {
        using (Mat backProj = new Mat())
        {
            Imgproc.calcBackProject(new List<Mat>(new Mat[] { hsvMat }), new MatOfInt(0), roiHistMat, backProj, new MatOfFloat(0, 180), 1.0);

            RotatedRect = Video.CamShift(backProj, ResultRect, termination);
            RotatedRect.points(roiPoints);
            RotatedRectPoints = roiPoints;
        }
    }

    private void SetNewTouchPoint(Mat img, Point touchPoint)
    {
        if (roiPointList.Count == 4)
            roiPointList.Clear();

        if (roiPointList.Count < 4)
        {
            roiPointList.Add(touchPoint);

            if (!(new Rect(0, 0, img.width(), img.height()).contains(roiPointList[roiPointList.Count - 1])))
                roiPointList.RemoveAt(roiPointList.Count - 1);

            if (roiPointList.Count == 4)
                shouldStartCamShift = true;
        }
    }

    private void ResultToTexture(Mat rgbaMat, Point[] points)
    {
        if (points.Length < 4)
        {
            for (int i = 0; i < points.Length; i++)
                Imgproc.circle(rgbaMat, points[i], 6, new Scalar(0, 0, 255, 255), 2);
        }
        else
        {
            // Rotated rectangle (made out of lines).
            for (int i = 0; i < 4; i++)
                Imgproc.line(rgbaMat, points[i], points[(i + 1) % 4], new Scalar(255, 0, 0, 255), 2);

            // Fixed rectangle.
            Imgproc.rectangle(rgbaMat, ResultRect.tl(), ResultRect.br(), new Scalar(255, 255, 0, 255), 2);
        }
        Utils.fastMatToTexture2D(rgbaMat, faceDetection.texture);
    }
}