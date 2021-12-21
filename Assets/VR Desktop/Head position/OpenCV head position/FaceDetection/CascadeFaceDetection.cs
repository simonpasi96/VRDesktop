using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using PositionsVector = System.Collections.Generic.List<OpenCVForUnity.CoreModule.Rect>;
using Rect = OpenCVForUnity.CoreModule.Rect;

/// <summary>
/// (based on AsynchronousFaceDetectionWebCamTextureExample)
/// </summary>
[RequireComponent(typeof(CustomWebCamTextureToMatHelper))]
public class CascadeFaceDetection : MonoBehaviour
{
    public CustomWebCamTextureToMatHelper WebCamTextureToMatHelper { get; private set; }
    [HideInInspector]
    public Texture2D texture;
    Mat grayMat;

    #region Cascade file ------------------
    CascadeClassifier cascade;
    protected static readonly string LBP_CASCADE_FILENAME = "lbpcascade_frontalface.xml";
    string lbp_cascade_filepath;
    protected static readonly string HAAR_CASCADE_FILENAME = "haarcascade_frontalface_alt.xml";
    string haar_cascade_filepath;
#if UNITY_WEBGL && !UNITY_EDITOR
    IEnumerator getFilePath_Coroutine;
#endif
    #endregion ----------------------------

    Rect[] RectsWhereRegions;
    List<Rect> detectedObjectsInRegions = new List<Rect>();
    List<Rect> resultObjects = new List<Rect>();

    CascadeClassifier cascade4Thread;
    Mat grayMatForThread;
    MatOfRect detectionResult;
    object sync = new object();

    #region Thread getters-setters ---------------
    bool _isThreadRunning = false;
    bool IsThreadRunning {
        get {
            lock (sync)
                return _isThreadRunning;
        }
        set {
            lock (sync)
                _isThreadRunning = value;
        }
    }

    bool _shouldStopThread = false;
    bool ShouldStopThread {
        get {
            lock (sync)
                return _shouldStopThread;
        }
        set {
            lock (sync)
                _shouldStopThread = value;
        }
    }

    bool _shouldDetectInMultiThread = false;
    bool ShouldDetectInMultiThread {
        get {
            lock (sync)
                return _shouldDetectInMultiThread;
        }
        set {
            lock (sync)
                _shouldDetectInMultiThread = value;
        }
    }

    bool _didUpdateTheDetectionResult = false;
    bool DidUpdateTheDetectionResult {
        get {
            lock (sync)
                return _didUpdateTheDetectionResult;
        }
        set {
            lock (sync)
                _didUpdateTheDetectionResult = value;
        }
    }
    #endregion ------------------------------

    List<TrackedObject> trackedObjects = new List<TrackedObject>();
    List<float> weightsPositionsSmoothing = new List<float>();
    List<float> weightsSizesSmoothing = new List<float>();
    Parameters parameters;
    InnerParameters innerParameters;
    [SerializeField]
    float minFaceSize = .1f; // (was at .2f by default)

    public Rect[] Rects { get; private set; }
    public bool DetectsAFace { get { return Rects != null && Rects.Length > 0; } }


    void Start()
    {
        WebCamTextureToMatHelper = gameObject.GetComponent<CustomWebCamTextureToMatHelper>();

#if UNITY_WEBGL && !UNITY_EDITOR
        getFilePath_Coroutine = GetFilePath ();
        StartCoroutine (getFilePath_Coroutine);
#else
        lbp_cascade_filepath = Utils.getFilePath(LBP_CASCADE_FILENAME);
        haar_cascade_filepath = Utils.getFilePath(HAAR_CASCADE_FILENAME);
        Run();
#endif
    }

    void Update()
    {
        if (WebCamTextureToMatHelper.IsPlaying() && WebCamTextureToMatHelper.DidUpdateThisFrame())
            UpdateDetection();
    }

    void OnDestroy()
    {
        WebCamTextureToMatHelper.Dispose();

#if UNITY_WEBGL && !UNITY_EDITOR
        if (getFilePath_Coroutine != null) {
            StopCoroutine (getFilePath_Coroutine);
            ((IDisposable)getFilePath_Coroutine).Dispose ();
        }
#endif
    }


#if UNITY_WEBGL && !UNITY_EDITOR
    private IEnumerator GetFilePath ()
    {
        var getFilePathAsync_lbpcascade_frontalface_xml_filepath_Coroutine = Utils.getFilePathAsync (LBP_CASCADE_FILENAME, (result) => {
            lbp_cascade_filepath = result;
        });
        yield return getFilePathAsync_lbpcascade_frontalface_xml_filepath_Coroutine;
            
        var getFilePathAsync_haarcascade_frontalface_alt_xml_filepath_Coroutine = Utils.getFilePathAsync (HAAR_CASCADE_FILENAME, (result) => {
            haar_cascade_filepath = result;
        });
        yield return getFilePathAsync_haarcascade_frontalface_alt_xml_filepath_Coroutine;
            
        getFilePath_Coroutine = null;
            
        Run ();
    }
#endif

    private void Run()
    {
        weightsPositionsSmoothing.Add(1);
        weightsSizesSmoothing.Add(0.5f);
        weightsSizesSmoothing.Add(0.3f);
        weightsSizesSmoothing.Add(0.2f);

        parameters.maxTrackLifetime = 5;

        innerParameters.numLastPositionsToTrack = 4;
        innerParameters.numStepsToWaitBeforeFirstShow = 6;
        innerParameters.numStepsToTrackWithoutDetectingIfObjectHasNotBeenShown = 3;
        innerParameters.numStepsToShowWithoutDetecting = 3;
        innerParameters.coeffTrackingWindowSize = 2.0f;
        innerParameters.coeffObjectSizeToTrack = 0.85f;
        innerParameters.coeffObjectSpeedUsingInPrediction = 0.8f;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
        WebCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
        WebCamTextureToMatHelper.Initialize();
    }

    public void OnWebCamTextureToMatHelperInitialized()
    {
        if (!WebCamTextureToMatHelper)
            Start();

        Mat webCamTextureMat = WebCamTextureToMatHelper.GetMat();

        texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);

        float width = webCamTextureMat.width();
        float height = webCamTextureMat.height();

        float widthScale = Screen.width / width;
        float heightScale = Screen.height / height;


        grayMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC1);
        cascade = new CascadeClassifier();
        cascade.load(lbp_cascade_filepath);
#if !UNITY_WSA_10_0
        if (cascade.empty())
            Debug.LogError("Cascade file is not loaded. Please copy from “OpenCVForUnity/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
#endif
        InitThread();
    }

    public void OnWebCamTextureToMatHelperDisposed()
    {
#if !UNITY_WEBGL
        StopThread();
#else
        StopCoroutine ("ThreadWorker");
#endif

        if (grayMatForThread != null)
            grayMatForThread.Dispose();

        if (cascade4Thread != null)
            cascade4Thread.Dispose();

        if (grayMat != null)
            grayMat.Dispose();

        if (texture != null)
        {
            Destroy(texture);
            texture = null;
        }

        if (cascade != null)
            cascade.Dispose();

        trackedObjects.Clear();
    }

    private void UpdateDetection()
    {
        Mat rgbaMat = WebCamTextureToMatHelper.GetMat();

        Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
        Imgproc.equalizeHist(grayMat, grayMat);

        if (!ShouldDetectInMultiThread)
        {
            grayMat.copyTo(grayMatForThread);
            ShouldDetectInMultiThread = true;
        }

        Rects = null;

        if (DidUpdateTheDetectionResult)
        {
            DidUpdateTheDetectionResult = false;
            RectsWhereRegions = detectionResult.toArray();
            Rects = RectsWhereRegions;
        }
        else
        {
            //Debug.Log("DetectionBasedTracker::process: get _RectsWhereRegions from previous positions");
            RectsWhereRegions = new Rect[trackedObjects.Count];

            for (int i = 0; i < trackedObjects.Count; i++)
            {
                int n = trackedObjects[i].lastPositions.Count;
                //if (n > 0) UnityEngine.Debug.LogError("n > 0 is false");

                Rect r = trackedObjects[i].lastPositions[n - 1].clone();
                if (r.area() == 0)
                {
                    Debug.Log("DetectionBasedTracker::process: ERROR: ATTENTION: strange algorithm's behavior: trackedObjects[i].rect() is empty");
                    continue;
                }

                //correction by speed of rectangle
                if (n > 1)
                {
                    Point center = CenterRect(r);
                    Point center_prev = CenterRect(trackedObjects[i].lastPositions[n - 2]);
                    Point shift = new Point((center.x - center_prev.x) * innerParameters.coeffObjectSpeedUsingInPrediction,
                                      (center.y - center_prev.y) * innerParameters.coeffObjectSpeedUsingInPrediction);

                    r.x += (int)Math.Round(shift.x);
                    r.y += (int)Math.Round(shift.y);
                }
                RectsWhereRegions[i] = r;
            }

            Rects = RectsWhereRegions;
        }

        detectedObjectsInRegions.Clear();
        if (RectsWhereRegions.Length > 0)
        {
            int len = RectsWhereRegions.Length;
            for (int i = 0; i < len; i++)
                DetectInRegion(grayMat, RectsWhereRegions[i], detectedObjectsInRegions);
        }

        UpdateTrackedObjects(detectedObjectsInRegions);
        GetObjects(resultObjects);

        Rects = resultObjects.ToArray();
    }

    private void ResultToTexture(Mat rgbaMat)
    {
        for (int i = 0; i < Rects.Length; i++)
            Imgproc.rectangle(rgbaMat, new Point(Rects[i].x, Rects[i].y), new Point(Rects[i].x + Rects[i].width, Rects[i].y + Rects[i].height), new Scalar(255, 0, 0, 255), 2);

        Utils.fastMatToTexture2D(rgbaMat, texture);
    }

    private void DetectInRegion(Mat img, Rect r, List<Rect> detectedObjectsInRegions)
    {
        Rect r0 = new Rect(new Point(), img.size());
        Rect r1 = new Rect(r.x, r.y, r.width, r.height);
        Rect.inflate(r1, (int)((r1.width * innerParameters.coeffTrackingWindowSize) - r1.width) / 2,
            (int)((r1.height * innerParameters.coeffTrackingWindowSize) - r1.height) / 2);
        r1 = Rect.intersect(r0, r1);

        if (r1 != null && (r1.width <= 0) || (r1.height <= 0))
        {
            Debug.Log("DetectionBasedTracker::detectInRegion: Empty intersection");
            return;
        }

        int d = Math.Min(r.width, r.height);
        d = (int)Math.Round(d * innerParameters.coeffObjectSizeToTrack);

        MatOfRect tmpobjects = new MatOfRect();
        Mat img1 = new Mat(img, r1); //subimage for rectangle -- without data copying
        cascade.detectMultiScale(img1, tmpobjects, 1.1, 2, 0 | Objdetect.CASCADE_DO_CANNY_PRUNING | Objdetect.CASCADE_SCALE_IMAGE | Objdetect.CASCADE_FIND_BIGGEST_OBJECT, new Size(d, d), new Size());

        Rect[] tmpobjectsArray = tmpobjects.toArray();
        for (int i = 0; i < tmpobjectsArray.Length; i++)
        {
            Rect tmp = tmpobjectsArray[i];
            Rect curres = new Rect(new Point(tmp.x + r1.x, tmp.y + r1.y), tmp.size());
            detectedObjectsInRegions.Add(curres);
        }
    }

    public static Point CenterRect(Rect r)
    {
        return new Point(r.x + (r.width / 2), r.y + (r.height / 2));
    }

    private void InitThread()
    {
        StopThread();

        grayMatForThread = new Mat();

        cascade4Thread = new CascadeClassifier();
        cascade4Thread.load(haar_cascade_filepath);
#if !UNITY_WSA_10_0
        if (cascade4Thread.empty())
            Debug.LogError("cascade4Thread file is not loaded. Please copy from “OpenCVForUnity/StreamingAssets/” to “Assets/StreamingAssets/” folder. ");
#endif

        ShouldDetectInMultiThread = false;

#if !UNITY_WEBGL
        StartThread(ThreadWorker);
#else
            StartCoroutine ("ThreadWorker");
#endif
    }

    private void StartThread(Action action)
    {
        ShouldStopThread = false;

#if UNITY_METRO && NETFX_CORE
            System.Threading.Tasks.Task.Run(() => action());
#elif UNITY_METRO
            action.BeginInvoke(ar => action.EndInvoke(ar), null);
#else
        ThreadPool.QueueUserWorkItem(_ => action());
#endif

        Debug.Log("Thread Start");
    }

    private void StopThread()
    {
        if (!IsThreadRunning)
            return;

        ShouldStopThread = true;

        // Wait threading stop.
        while (IsThreadRunning) ;

        Debug.Log("Thread Stop");
    }

#if !UNITY_WEBGL
    private void ThreadWorker()
    {
        IsThreadRunning = true;

        while (!ShouldStopThread)
        {
            if (!ShouldDetectInMultiThread)
                continue;

            Detect();

            ShouldDetectInMultiThread = false;
            DidUpdateTheDetectionResult = true;
        }

        IsThreadRunning = false;
    }

#else
        private IEnumerator ThreadWorker ()
        {
            while (true) {
                while (!shouldDetectInMultiThread) {
                    yield return null;
                }

                Detect ();

                shouldDetectInMultiThread = false;
                didUpdateTheDetectionResult = true;
            }
        }
#endif

    private void Detect()
    {
        MatOfRect objects = new MatOfRect();
        if (cascade4Thread != null)
            cascade4Thread.detectMultiScale(grayMatForThread, objects, 1.1, 2, Objdetect.CASCADE_SCALE_IMAGE, // TODO: objdetect.CV_HAAR_SCALE_IMAGE
                new Size(grayMatForThread.height() * minFaceSize, grayMatForThread.height() * minFaceSize), new Size());
        // ↑ Ici on détecte des choses dans un Mat. Il y a peut être moyen de cibler ce Mat vers l'endroit où était la dernière tête trouvée.

        detectionResult = objects;
    }

    private void GetObjects(List<Rect> result)
    {
        result.Clear();

        for (int i = 0; i < trackedObjects.Count; i++)
        {
            Rect r = CalcTrackedObjectPositionToShow(i);
            if (r.area() == 0)
            {
                continue;
            }
            result.Add(r);
            //LOGD("DetectionBasedTracker::process: found a object with SIZE %d x %d, rect={%d, %d, %d x %d}", r.width, r.height, r.x, r.y, r.width, r.height);
        }
    }

    private enum TrackedState : int
    {
        NEW_RECTANGLE = -1,
        INTERSECTED_RECTANGLE = -2
    }

    private void UpdateTrackedObjects(List<Rect> detectedObjects)
    {
        int N1 = trackedObjects.Count;
        int N2 = detectedObjects.Count;

        for (int i = 0; i < N1; i++)
            trackedObjects[i].numDetectedFrames++;

        int[] correspondence = new int[N2];
        for (int i = 0; i < N2; i++)
            correspondence[i] = (int)TrackedState.NEW_RECTANGLE;


        for (int i = 0; i < N1; i++)
        {
            TrackedObject curObject = trackedObjects[i];

            int bestIndex = -1;
            int bestArea = -1;

            int numpositions = curObject.lastPositions.Count;

            //if (numpositions > 0) UnityEngine.Debug.LogError("numpositions > 0 is false");

            Rect prevRect = curObject.lastPositions[numpositions - 1];

            for (int j = 0; j < N2; j++)
            {
                if (correspondence[j] >= 0)
                {
                    //Debug.Log("DetectionBasedTracker::updateTrackedObjects: j=" + i + " is rejected, because it has correspondence=" + correspondence[j]);
                    continue;
                }
                if (correspondence[j] != (int)TrackedState.NEW_RECTANGLE)
                {
                    //Debug.Log("DetectionBasedTracker::updateTrackedObjects: j=" + j + " is rejected, because it is intersected with another rectangle");
                    continue;
                }

                Rect r = Rect.intersect(prevRect, detectedObjects[j]);
                if (r != null && (r.width > 0) && (r.height > 0))
                {
                    //LOGD("DetectionBasedTracker::updateTrackedObjects: There is intersection between prevRect and detectedRect, r={%d, %d, %d x %d}",
                    //        r.x, r.y, r.width, r.height);
                    correspondence[j] = (int)TrackedState.INTERSECTED_RECTANGLE;

                    if (r.area() > bestArea)
                    {
                        //LOGD("DetectionBasedTracker::updateTrackedObjects: The area of intersection is %d, it is better than bestArea=%d", r.area(), bestArea);
                        bestIndex = j;
                        bestArea = (int)r.area();
                    }
                }
            }

            if (bestIndex >= 0)
            {
                //LOGD("DetectionBasedTracker::updateTrackedObjects: The best correspondence for i=%d is j=%d", i, bestIndex);
                correspondence[bestIndex] = i;

                for (int j = 0; j < N2; j++)
                {
                    if (correspondence[j] >= 0)
                        continue;

                    Rect r = Rect.intersect(detectedObjects[j], detectedObjects[bestIndex]);
                    if (r != null && (r.width > 0) && (r.height > 0))
                    {
                        //LOGD("DetectionBasedTracker::updateTrackedObjects: Found intersection between "
                        //    "rectangles j=%d and bestIndex=%d, rectangle j=%d is marked as intersected", j, bestIndex, j);
                        correspondence[j] = (int)TrackedState.INTERSECTED_RECTANGLE;
                    }
                }
            }
            else
            {
                //LOGD("DetectionBasedTracker::updateTrackedObjects: There is no correspondence for i=%d ", i);
                curObject.numFramesNotDetected++;
            }
        }

        //LOGD("DetectionBasedTracker::updateTrackedObjects: start second cycle");
        for (int j = 0; j < N2; j++)
        {
            int i = correspondence[j];
            if (i >= 0)
            {
                //add position
                //Debug.Log("DetectionBasedTracker::updateTrackedObjects: add position");
                trackedObjects[i].lastPositions.Add(detectedObjects[j]);
                while ((int)trackedObjects[i].lastPositions.Count > (int)innerParameters.numLastPositionsToTrack)
                    trackedObjects[i].lastPositions.Remove(trackedObjects[i].lastPositions[0]);
                trackedObjects[i].numFramesNotDetected = 0;
            }
            else if (i == (int)TrackedState.NEW_RECTANGLE)
                //new object
                trackedObjects.Add(new TrackedObject(detectedObjects[j]));
            else
            {
                //Debug.Log ("DetectionBasedTracker::updateTrackedObjects: was auxiliary intersection");
            }
        }

        int t = 0;
        TrackedObject trackedObject;
        while (t < trackedObjects.Count)
        {
            trackedObject = trackedObjects[t];

            if ((trackedObject.numFramesNotDetected > parameters.maxTrackLifetime)
                ||
                ((trackedObject.numDetectedFrames <= innerParameters.numStepsToWaitBeforeFirstShow)
                &&
                (trackedObject.numFramesNotDetected > innerParameters.numStepsToTrackWithoutDetectingIfObjectHasNotBeenShown)))
            {
                trackedObjects.Remove(trackedObject);
            }
            else
                t++;
        }
    }

    private Rect CalcTrackedObjectPositionToShow(int i)
    {
        if ((i < 0) || (i >= trackedObjects.Count))
        {
            Debug.Log("DetectionBasedTracker::calcTrackedObjectPositionToShow: ERROR: wrong i=" + i);
            return new Rect();
        }
        if (trackedObjects[i].numDetectedFrames <= innerParameters.numStepsToWaitBeforeFirstShow)
            return new Rect();
        if (trackedObjects[i].numFramesNotDetected > innerParameters.numStepsToShowWithoutDetecting)
            return new Rect();

        List<Rect> lastPositions = trackedObjects[i].lastPositions;

        int N = lastPositions.Count;
        if (N <= 0)
        {
            Debug.Log("DetectionBasedTracker::calcTrackedObjectPositionToShow: ERROR: no positions for i=" + i);
            return new Rect();
        }

        int Nsize = Math.Min(N, (int)weightsSizesSmoothing.Count);
        int Ncenter = Math.Min(N, (int)weightsPositionsSmoothing.Count);

        Point center = new Point();
        double w = 0, h = 0;
        if (Nsize > 0)
        {
            double sum = 0;
            for (int j = 0; j < Nsize; j++)
            {
                int k = N - j - 1;
                w += lastPositions[k].width * weightsSizesSmoothing[j];
                h += lastPositions[k].height * weightsSizesSmoothing[j];
                sum += weightsSizesSmoothing[j];
            }
            w /= sum;
            h /= sum;
        }
        else
        {
            w = lastPositions[N - 1].width;
            h = lastPositions[N - 1].height;
        }

        if (Ncenter > 0)
        {
            double sum = 0;
            for (int j = 0; j < Ncenter; j++)
            {
                int k = N - j - 1;
                Point tl = lastPositions[k].tl();
                Point br = lastPositions[k].br();
                Point c1;
                c1 = new Point(tl.x * 0.5f, tl.y * 0.5f);
                Point c2;
                c2 = new Point(br.x * 0.5f, br.y * 0.5f);
                c1 = new Point(c1.x + c2.x, c1.y + c2.y);

                center = new Point(center.x + (c1.x * weightsPositionsSmoothing[j]), center.y + (c1.y * weightsPositionsSmoothing[j]));
                sum += weightsPositionsSmoothing[j];
            }
            center = new Point(center.x * (1 / sum), center.y * (1 / sum));
        }
        else
        {
            int k = N - 1;
            Point tl = lastPositions[k].tl();
            Point br = lastPositions[k].br();
            Point c1;
            c1 = new Point(tl.x * 0.5f, tl.y * 0.5f);
            Point c2;
            c2 = new Point(br.x * 0.5f, br.y * 0.5f);

            center = new Point(c1.x + c2.x, c1.y + c2.y);
        }
        Point tl2 = new Point(center.x - (w * 0.5f), center.y - (h * 0.5f));
        Rect res = new Rect((int)Math.Round(tl2.x), (int)Math.Round(tl2.y), (int)Math.Round(w), (int)Math.Round(h));

        return res;
    }

    private struct Parameters
    {
        public int maxTrackLifetime;
    };

    private struct InnerParameters
    {
        public int numLastPositionsToTrack;
        public int numStepsToWaitBeforeFirstShow;
        public int numStepsToTrackWithoutDetectingIfObjectHasNotBeenShown;
        public int numStepsToShowWithoutDetecting;
        public float coeffTrackingWindowSize;
        public float coeffObjectSizeToTrack;
        public float coeffObjectSpeedUsingInPrediction;
    };

    private class TrackedObject
    {
        public PositionsVector lastPositions;
        public int numDetectedFrames;
        public int numFramesNotDetected;
        public int id;
        static private int _id = 0;

        public TrackedObject(Rect rect)
        {
            lastPositions = new PositionsVector();

            numDetectedFrames = 1;
            numFramesNotDetected = 0;

            lastPositions.Add(rect.clone());

            _id = GetNextId();
            id = _id;
        }

        static int GetNextId()
        {
            _id++;
            return _id;
        }
    }
}