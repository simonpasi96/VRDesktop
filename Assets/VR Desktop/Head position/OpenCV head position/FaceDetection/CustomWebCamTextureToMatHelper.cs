using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace OpenCVForUnity.UnityUtils.Helper
{
    /// <summary>
    /// WebcamTexture to mat helper.
    /// v 1.1.0
    /// </summary>
    public class CustomWebCamTextureToMatHelper : MonoBehaviour
    {
        /// <summary>
        /// Set the name of the camera device to use. (or device index number)
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedDeviceName"), TooltipAttribute("Set the name of the device to use. (or device index number)")]
        protected string _requestedDeviceName = null;
        public virtual string RequestedDeviceName {
            get { return _requestedDeviceName; }
            set {
                _requestedDeviceName = value;
                if (hasInitDone)
                    Initialize();
            }
        }

        /// <summary>
        /// Set the width of camera.
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedWidth"), TooltipAttribute("Set the width of camera.")]
        protected int _requestedWidth = 640;
        public virtual int RequestedWidth {
            get { return _requestedWidth; }
            set {
                _requestedWidth = (int)Mathf.Clamp(value, 0f, float.MaxValue);
                if (hasInitDone)
                    Initialize();
            }
        }

        /// <summary>
        /// Set the height of camera.
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedHeight"), TooltipAttribute("Set the height of camera.")]
        protected int _requestedHeight = 480;
        public virtual int RequestedHeight {
            get { return _requestedHeight; }
            set {
                _requestedHeight = (int)Mathf.Clamp(value, 0f, float.MaxValue);
                if (hasInitDone)
                    Initialize();
            }
        }

        /// <summary>
        /// Set whether to use the front facing camera.
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedIsFrontFacing"), TooltipAttribute("Set whether to use the front facing camera.")]
        protected bool _requestedIsFrontFacing = false;
        public virtual bool RequestedIsFrontFacing {
            get { return _requestedIsFrontFacing; }
            set {
                _requestedIsFrontFacing = value;
                if (hasInitDone)
                    Initialize(_requestedIsFrontFacing, RequestedFPS, Rotate90Degree);
            }
        }

        /// <summary>
        /// Set the frame rate of camera.
        /// </summary>
        [SerializeField, FormerlySerializedAs("requestedFPS"), TooltipAttribute("Set the frame rate of camera.")]
        protected float _requestedFPS = 30f;
        public virtual float RequestedFPS {
            get { return _requestedFPS; }
            set {
                _requestedFPS = Mathf.Clamp(value, -1f, float.MaxValue);
                if (hasInitDone)
                {
                    bool isPlaying = IsPlaying();
                    Stop();
                    WebCamTexture.requestedFPS = _requestedFPS;
                    if (isPlaying)
                        Play();
                }
            }
        }

        /// <summary>
        /// Sets whether to rotate camera frame 90 degrees. (clockwise)
        /// </summary>
        [SerializeField, FormerlySerializedAs("rotate90Degree"), TooltipAttribute("Sets whether to rotate camera frame 90 degrees. (clockwise)")]
        protected bool _rotate90Degree = false;
        public virtual bool Rotate90Degree {
            get { return _rotate90Degree; }
            set {
                _rotate90Degree = value;
                if (hasInitDone)
                    Initialize();
            }
        }

        /// <summary>
        /// Determines if flips vertically.
        /// </summary>
        [SerializeField, FormerlySerializedAs("flipVertical"), TooltipAttribute("Determines if flips vertically.")]
        protected bool _flipVertical = false;
        public virtual bool FlipVertical {
            get { return _flipVertical; }
            set { _flipVertical = value; }
        }

        /// <summary>
        /// Determines if flips horizontal.
        /// </summary>
        [SerializeField, FormerlySerializedAs("flipHorizontal"), TooltipAttribute("Determines if flips horizontal.")]
        protected bool _flipHorizontal = false;
        public virtual bool FlipHorizontal {
            get { return _flipHorizontal; }
            set { _flipHorizontal = value; }
        }

        /// <summary>
        /// The number of frames before the initialization process times out.
        /// </summary>
        [SerializeField, FormerlySerializedAs("timeoutFrameCount"), TooltipAttribute("The number of frames before the initialization process times out.")]
        protected int _timeoutFrameCount = 300;
        public virtual int TimeoutFrameCount {
            get { return _timeoutFrameCount; }
            set { _timeoutFrameCount = (int)Mathf.Clamp(value, 0f, float.MaxValue); }
        }

        public UnityEvent OnInitialized;
        public UnityEvent OnDisposed;
        public ErrorUnityEvent OnErrorOccurred;
        public WebCamTexture WebCamTexture { get; private set; }

        /// <summary>
        /// The active WebcamDevice.
        /// </summary>
        protected WebCamDevice webCamDevice;
        protected Mat frameMat;
        protected Mat rotatedFrameMat;

        /// <summary>
        /// The buffer colors.
        /// </summary>
        protected Color32[] colors;

        protected bool isInitWaiting = false;
        protected bool hasInitDone = false;
        protected IEnumerator initCoroutine;

        protected ScreenOrientation screenOrientation;
        protected int screenWidth;
        protected int screenHeight;

        /// <summary>
        /// Indicates whether this instance avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
        /// Sets compulsorily the requestedFPS parameter to 15 (only when using the front camera), to avoid the problem of the WebCamTexture image becoming low light.
        /// https://forum.unity.com/threads/android-webcamtexture-in-low-light-only-some-models.520656/
        /// https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
        /// </summary>
        public bool avoidAndroidFrontCameraLowLightIssue = false;

        [Serializable]
        public enum ErrorCode : int
        {
            UNKNOWN = 0,
            CAMERA_DEVICE_NOT_EXIST = 1,
            TIMEOUT = 2,
        }

        [Serializable]
        public class ErrorUnityEvent : UnityEvent<ErrorCode> { }

        
        protected virtual void OnValidate()
        {
            _requestedWidth = (int)Mathf.Clamp(_requestedWidth, 0f, float.MaxValue);
            _requestedHeight = (int)Mathf.Clamp(_requestedHeight, 0f, float.MaxValue);
            _requestedFPS = Mathf.Clamp(_requestedFPS, -1f, float.MaxValue);
            _timeoutFrameCount = (int)Mathf.Clamp(_timeoutFrameCount, 0f, float.MaxValue);
        }

        protected virtual void Update()
        {
            if (!hasInitDone)
                return;

            // Catch the orientation change of the screen and correct the mat image to the correct direction.
            if (screenOrientation != Screen.orientation && (screenWidth != Screen.width || screenHeight != Screen.height))
                CorrectOrientation();
            else
            {
                screenWidth = Screen.width;
                screenHeight = Screen.height;
            }
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }


        void CorrectOrientation()
        {
            if (OnDisposed != null)
                OnDisposed.Invoke();

            if (frameMat != null)
            {
                frameMat.Dispose();
                frameMat = null;
            }
            if (rotatedFrameMat != null)
            {
                rotatedFrameMat.Dispose();
                rotatedFrameMat = null;
            }

            frameMat = new Mat(WebCamTexture.height, WebCamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
            screenOrientation = Screen.orientation;
            screenWidth = Screen.width;
            screenHeight = Screen.height;

            bool isRotatedFrame = false;
#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
                    if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                    {
                        if (!Rotate90Degree)
                            isRotatedFrame = true;
                    }
                    else if (Rotate90Degree)
                    {
                        isRotatedFrame = true;
                    }
#else
            if (Rotate90Degree)
                isRotatedFrame = true;
#endif
            if (isRotatedFrame)
                rotatedFrameMat = new Mat(WebCamTexture.width, WebCamTexture.height, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));

            if (OnInitialized != null)
                OnInitialized.Invoke();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public virtual void Initialize()
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            if (OnDisposed == null)
                OnDisposed = new UnityEvent();
            if (OnErrorOccurred == null)
                OnErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="requestedWidth">Requested width.</param>
        /// <param name="requestedHeight">Requested height.</param>
        public virtual void Initialize(int requestedWidth, int requestedHeight)
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            _requestedWidth = requestedWidth;
            _requestedHeight = requestedHeight;
            if (OnDisposed == null)
                OnDisposed = new UnityEvent();
            if (OnErrorOccurred == null)
                OnErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="requestedIsFrontFacing">If set to <c>true</c> requested to using the front camera.</param>
        /// <param name="requestedFPS">Requested FPS.</param>
        /// <param name="rotate90Degree">If set to <c>true</c> requested to rotate camera frame 90 degrees. (clockwise)</param>
        public virtual void Initialize(bool requestedIsFrontFacing, float requestedFPS = 30f, bool rotate90Degree = false)
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            _requestedDeviceName = null;
            _requestedIsFrontFacing = requestedIsFrontFacing;
            _requestedFPS = requestedFPS;
            _rotate90Degree = rotate90Degree;
            if (OnErrorOccurred == null)
                OnErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="requestedWidth"></param>
        /// <param name="requestedHeight"></param>
        /// <param name="requestedIsFrontFacing">If set to <c>true</c> requested to using the front camera.</param>
        /// <param name="requestedFPS"></param>
        /// <param name="rotate90Degree">If set to <c>true</c> requested to rotate camera frame 90 degrees. (clockwise)</param>
        public virtual void Initialize(string deviceName, int requestedWidth, int requestedHeight, bool requestedIsFrontFacing = false, float requestedFPS = 30f, bool rotate90Degree = false)
        {
            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }

            _requestedDeviceName = deviceName;
            _requestedWidth = requestedWidth;
            _requestedHeight = requestedHeight;
            _requestedIsFrontFacing = requestedIsFrontFacing;
            _requestedFPS = requestedFPS;
            _rotate90Degree = rotate90Degree;
            if (OnErrorOccurred == null)
                OnErrorOccurred = new ErrorUnityEvent();

            initCoroutine = _Initialize();
            StartCoroutine(initCoroutine);
        }

        /// <summary>
        /// Initializes this instance by coroutine.
        /// </summary>
        protected virtual IEnumerator _Initialize()
        {
            if (hasInitDone)
            {
                ReleaseResources();

                if (OnDisposed != null)
                    OnDisposed.Invoke();
            }

            isInitWaiting = true;

            float requestedFPS = RequestedFPS;

            // Creates camera from requested name.
            if (!string.IsNullOrEmpty(RequestedDeviceName))
            {
                int requestedDeviceIndex = -1;
                if (int.TryParse(RequestedDeviceName, out requestedDeviceIndex))
                {
                    if (requestedDeviceIndex >= 0 && requestedDeviceIndex < WebCamTexture.devices.Length)
                    {
                        webCamDevice = WebCamTexture.devices[requestedDeviceIndex];

                        if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                            requestedFPS = 15f;

                        if (requestedFPS < 0)
                            WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight);
                        else
                            WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight, (int)requestedFPS);
                    }
                }
                else
                {
                    for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
                    {
                        if (WebCamTexture.devices[cameraIndex].name == RequestedDeviceName)
                        {
                            webCamDevice = WebCamTexture.devices[cameraIndex];

                            if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                                requestedFPS = 15f;

                            if (requestedFPS < 0)
                                WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight);
                            else
                                WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight, (int)requestedFPS);
                            break;
                        }
                    }
                }
                if (WebCamTexture == null)
                    Debug.Log("Cannot find camera device " + RequestedDeviceName + ".");
            }

            if (WebCamTexture == null)
            {
                // Checks how many and which cameras are available on the device.
                for (int i = 0; i < WebCamTexture.devices.Length; i++)
                {
                    if (WebCamTexture.devices[i].isFrontFacing != RequestedIsFrontFacing)
                        continue;

                    webCamDevice = WebCamTexture.devices[i];

                    if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                        requestedFPS = 15f;
                    if (requestedFPS < 0)
                        WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight);
                    else
                        WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight, (int)requestedFPS);
                    break;
                }
            }

            if (WebCamTexture == null)
            {
                if (WebCamTexture.devices.Length > 0)
                {
                    webCamDevice = WebCamTexture.devices[0];

                    if (avoidAndroidFrontCameraLowLightIssue && webCamDevice.isFrontFacing == true)
                        requestedFPS = 15f;

                    if (requestedFPS < 0)
                        WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight);
                    else
                        WebCamTexture = new WebCamTexture(webCamDevice.name, RequestedWidth, RequestedHeight, (int)requestedFPS);
                }
                else
                {
                    isInitWaiting = false;

                    if (OnErrorOccurred != null)
                        OnErrorOccurred.Invoke(ErrorCode.CAMERA_DEVICE_NOT_EXIST);

                    yield break;
                }
            }

            // Starts the camera
            WebCamTexture.Play();

            int initFrameCount = 0;
            bool isTimeout = false;

            while (true)
            {
                if (initFrameCount > TimeoutFrameCount)
                {
                    isTimeout = true;
                    break;
                }
#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
                // If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
                else if (webCamTexture.width > 16 && webCamTexture.height > 16) {
#else
                else if (WebCamTexture.didUpdateThisFrame)
                {
#if UNITY_IOS && !UNITY_EDITOR && UNITY_5_2
                    while (WebCamTexture.width <= 16)
                    {
                        if (initFrameCount > TimeoutFrameCount)
                        {
                            isTimeout = true;
                            break;
                        }
                        else
                            initFrameCount++;
                        WebCamTexture.GetPixels32();
                        yield return new WaitForEndOfFrame();
                    }
                    if (isTimeout) break;
#endif
#endif

                    Debug.Log("WebCamTextureToMatHelper:: " + "devicename:" + WebCamTexture.deviceName + " name:" + WebCamTexture.name + " width:" + WebCamTexture.width + " height:" + WebCamTexture.height + " fps:" + WebCamTexture.requestedFPS
                    + " videoRotationAngle:" + WebCamTexture.videoRotationAngle + " videoVerticallyMirrored:" + WebCamTexture.videoVerticallyMirrored + " isFrongFacing:" + webCamDevice.isFrontFacing);

                    if (colors == null || colors.Length != WebCamTexture.width * WebCamTexture.height)
                        colors = new Color32[WebCamTexture.width * WebCamTexture.height];

                    frameMat = new Mat(WebCamTexture.height, WebCamTexture.width, CvType.CV_8UC4);
                    screenOrientation = Screen.orientation;
                    screenWidth = Screen.width;
                    screenHeight = Screen.height;

                    bool isRotatedFrame = false;
#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
                    if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                    {
                        if (!Rotate90Degree)
                            isRotatedFrame = true;
                    }
                    else if (Rotate90Degree)
                    {
                        isRotatedFrame = true;
                    }
#else
                    if (Rotate90Degree)
                        isRotatedFrame = true;
#endif
                    if (isRotatedFrame)
                        rotatedFrameMat = new Mat(WebCamTexture.width, WebCamTexture.height, CvType.CV_8UC4);

                    isInitWaiting = false;
                    hasInitDone = true;
                    initCoroutine = null;

                    if (OnInitialized != null)
                        OnInitialized.Invoke();

                    break;
                }
                else
                {
                    initFrameCount++;
                    yield return null;
                }
            }

            if (isTimeout)
            {
                WebCamTexture.Stop();
                WebCamTexture = null;
                isInitWaiting = false;
                initCoroutine = null;

                if (OnErrorOccurred != null)
                    OnErrorOccurred.Invoke(ErrorCode.TIMEOUT);
            }
        }

        /// <summary>
        /// Indicates whether this instance has been initialized.
        /// </summary>
        /// <returns><c>true</c>, if this instance has been initialized, <c>false</c> otherwise.</returns>
        public virtual bool IsInitialized()
        {
            return hasInitDone;
        }

        /// <summary>
        /// Starts the camera.
        /// </summary>
        public virtual void Play()
        {
            if (hasInitDone)
                WebCamTexture.Play();
        }

        /// <summary>
        /// Pauses the active camera.
        /// </summary>
        public virtual void Pause()
        {
            if (hasInitDone)
                WebCamTexture.Pause();
        }

        /// <summary>
        /// Stops the active camera.
        /// </summary>
        public virtual void Stop()
        {
            if (hasInitDone)
                WebCamTexture.Stop();
        }

        /// <summary>
        /// Indicates whether the active camera is currently playing.
        /// </summary>
        /// <returns><c>true</c>, if the active camera is playing, <c>false</c> otherwise.</returns>
        public virtual bool IsPlaying()
        {
            return hasInitDone ? WebCamTexture.isPlaying : false;
        }

        /// <summary>
        /// Indicates whether the active camera device is currently front facng.
        /// </summary>
        /// <returns><c>true</c>, if the active camera device is front facng, <c>false</c> otherwise.</returns>
        public virtual bool IsFrontFacing()
        {
            return hasInitDone ? webCamDevice.isFrontFacing : false;
        }

        /// <summary>
        /// Returns the active camera device name.
        /// </summary>
        /// <returns>The active camera device name.</returns>
        public virtual string GetDeviceName()
        {
            return hasInitDone ? WebCamTexture.deviceName : "";
        }

        /// <summary>
        /// Returns the active camera width.
        /// </summary>
        /// <returns>The active camera width.</returns>
        public virtual int GetWidth()
        {
            if (!hasInitDone)
                return -1;
            return (rotatedFrameMat != null) ? frameMat.height() : frameMat.width();
        }

        /// <summary>
        /// Returns the active camera height.
        /// </summary>
        /// <returns>The active camera height.</returns>
        public virtual int GetHeight()
        {
            if (!hasInitDone)
                return -1;
            return (rotatedFrameMat != null) ? frameMat.width() : frameMat.height();
        }

        /// <summary>
        /// Returns the active camera framerate.
        /// </summary>
        /// <returns>The active camera framerate.</returns>
        public virtual float GetFPS()
        {
            return hasInitDone ? WebCamTexture.requestedFPS : -1f;
        }

        /// <summary>
        /// Returns the active WebcamTexture.
        /// </summary>
        /// <returns>The active WebcamTexture.</returns>
        public virtual WebCamTexture GetWebCamTexture()
        {
            return hasInitDone ? WebCamTexture : null;
        }

        /// <summary>
        /// Returns the active WebcamDevice.
        /// </summary>
        /// <returns>The active WebcamDevice.</returns>
        public virtual WebCamDevice GetWebCamDevice()
        {
            return webCamDevice;
        }

        /// <summary>
        /// Returns the camera to world matrix.
        /// </summary>
        /// <returns>The camera to world matrix.</returns>
        public virtual Matrix4x4 GetCameraToWorldMatrix()
        {
            return Camera.main.cameraToWorldMatrix;
        }

        /// <summary>
        /// Returns the projection matrix matrix.
        /// </summary>
        /// <returns>The projection matrix.</returns>
        public virtual Matrix4x4 GetProjectionMatrix()
        {
            return Camera.main.projectionMatrix;
        }

        /// <summary>
        /// Indicates whether the video buffer of the frame has been updated.
        /// </summary>
        /// <returns><c>true</c>, if the video buffer has been updated <c>false</c> otherwise.</returns>
        public virtual bool DidUpdateThisFrame()
        {
            if (!hasInitDone)
                return false;

#if UNITY_IOS && !UNITY_EDITOR && (UNITY_4_6_3 || UNITY_4_6_4 || UNITY_5_0_0 || UNITY_5_0_1)
            if (webCamTexture.width > 16 && webCamTexture.height > 16)
                return true;
            else
                return false;
#else
            return WebCamTexture.didUpdateThisFrame;
#endif
        }

        /// <summary>
        /// Gets the mat of the current frame.
        /// The Mat object's type is 'CV_8UC4' (RGBA).
        /// </summary>
        /// <returns>The mat of the current frame.</returns>
        public virtual Mat GetMat()
        {
            if (!hasInitDone || !WebCamTexture.isPlaying)
                return (rotatedFrameMat != null) ? rotatedFrameMat : frameMat;

            Utils.webCamTextureToMat(WebCamTexture, frameMat, colors, false);

#if !UNITY_EDITOR && !(UNITY_STANDALONE || UNITY_WEBGL)
            if (rotatedFrameMat != null)
            {
                if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                {
                    // (Orientation is Portrait, rotate90Degree is false)
                    if (webCamDevice.isFrontFacing)
                    {
                        FlipMat (frameMat, !FlipHorizontal, !FlipVertical);
                    }
                    else
                    {
                        FlipMat (frameMat, FlipHorizontal, FlipVertical);
                    }
                }
                else
                {
                    // (Orientation is Landscape, rotate90Degrees=true)
                    FlipMat (frameMat, FlipVertical, FlipHorizontal);
                }
                Core.rotate (frameMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                return rotatedFrameMat;
            }
            else
            {
                if (screenOrientation == ScreenOrientation.Portrait || screenOrientation == ScreenOrientation.PortraitUpsideDown)
                {
                    // (Orientation is Portrait, rotate90Degree is ture)
                    if (webCamDevice.isFrontFacing)
                    {
                        FlipMat (frameMat, FlipHorizontal, FlipVertical);
                    }
                    else
                    {
                        FlipMat (frameMat, !FlipHorizontal, !FlipVertical);
                    }
                }
                else
                {
                    // (Orientation is Landscape, rotate90Degree is false)
                    FlipMat (frameMat, FlipVertical, FlipHorizontal);
                }
                return frameMat;
            }
#else
            FlipMat(frameMat, FlipVertical, FlipHorizontal);
            if (rotatedFrameMat != null)
            {
                Core.rotate(frameMat, rotatedFrameMat, Core.ROTATE_90_CLOCKWISE);
                return rotatedFrameMat;
            }
            else
                return frameMat;
#endif
        }

        /// <summary>
        /// Flips the mat.
        /// </summary>
        /// <param name="mat">Mat.</param>
        protected virtual void FlipMat(Mat mat, bool flipVertical, bool flipHorizontal)
        {
            //Since the order of pixels of WebCamTexture and Mat is opposite, the initial value of flipCode is set to 0 (flipVertical).
            int flipCode = 0;

            if (webCamDevice.isFrontFacing)
            {
                if (WebCamTexture.videoRotationAngle == 0)
                {
                    flipCode = -1;
                }
                else if (WebCamTexture.videoRotationAngle == 90)
                {
                    flipCode = -1;
                }
                if (WebCamTexture.videoRotationAngle == 180)
                {
                    flipCode = int.MinValue;
                }
                else if (WebCamTexture.videoRotationAngle == 270)
                {
                    flipCode = int.MinValue;
                }
            }
            else
            {
                if (WebCamTexture.videoRotationAngle == 180)
                {
                    flipCode = 1;
                }
                else if (WebCamTexture.videoRotationAngle == 270)
                {
                    flipCode = 1;
                }
            }

            if (flipVertical)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 0;
                }
                else if (flipCode == 0)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == 1)
                {
                    flipCode = -1;
                }
                else if (flipCode == -1)
                {
                    flipCode = 1;
                }
            }

            if (flipHorizontal)
            {
                if (flipCode == int.MinValue)
                {
                    flipCode = 1;
                }
                else if (flipCode == 0)
                {
                    flipCode = -1;
                }
                else if (flipCode == 1)
                {
                    flipCode = int.MinValue;
                }
                else if (flipCode == -1)
                {
                    flipCode = 0;
                }
            }

            if (flipCode > int.MinValue)
            {
                Core.flip(mat, mat, flipCode);
            }
        }

        /// <summary>
        /// Gets the buffer colors.
        /// </summary>
        /// <returns>The buffer colors.</returns>
        public virtual Color32[] GetBufferColors()
        {
            return colors;
        }

        /// <summary>
        /// Cancel Init Coroutine.
        /// </summary>
        protected virtual void CancelInitCoroutine()
        {
            if (initCoroutine != null)
            {
                StopCoroutine(initCoroutine);
                ((IDisposable)initCoroutine).Dispose();
                initCoroutine = null;
            }
        }

        /// <summary>
        /// To release the resources.
        /// </summary>
        protected virtual void ReleaseResources()
        {
            isInitWaiting = false;
            hasInitDone = false;

            if (WebCamTexture != null)
            {
                WebCamTexture.Stop();
                WebCamTexture.Destroy(WebCamTexture);
                WebCamTexture = null;
            }
            if (frameMat != null)
            {
                frameMat.Dispose();
                frameMat = null;
            }
            if (rotatedFrameMat != null)
            {
                rotatedFrameMat.Dispose();
                rotatedFrameMat = null;
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="WebCamTextureToMatHelper"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="WebCamTextureToMatHelper"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="WebCamTextureToMatHelper"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="WebCamTextureToMatHelper"/> so
        /// the garbage collector can reclaim the memory that the <see cref="WebCamTextureToMatHelper"/> was occupying.</remarks>
        public virtual void Dispose()
        {
            if (colors != null)
                colors = null;

            if (isInitWaiting)
            {
                CancelInitCoroutine();
                ReleaseResources();
            }
            else if (hasInitDone)
            {
                ReleaseResources();

                if (OnDisposed != null)
                    OnDisposed.Invoke();
            }
        }
    }
}