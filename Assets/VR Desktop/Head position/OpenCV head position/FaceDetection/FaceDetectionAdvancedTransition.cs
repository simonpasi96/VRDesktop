using OpenCVForUnity.CoreModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rect = OpenCVForUnity.CoreModule.Rect;

public class FaceDetectionAdvancedTransition : FaceDetection
{
    protected CustomRect resultFaceRect;
    public override CustomRect CurrentFaceRect {
        get {
            return resultFaceRect;
        }
    }
    protected Rect lastFaceDetectionRect;
    protected CustomRect lastCamshiftRect;
    protected CustomRect lastCorrectedCamshiftRect;
    CustomRect CamshiftRectForResult { get { return lastCorrectedCamshiftRect ?? lastCamshiftRect; } }

    Point faceDToCamshiftCenterOffset;
    float faceDToCamshiftWidthOffset;
    bool gotOffsets = false;

    float toCamshiftTransitionDuration = 0;
    float fromCamshiftTransitionDuration = .2f;
    /// <summary>
    /// The smoothed transition between the camshift detection and the face detection.
    /// </summary>
    IEnumerator transition = null;

    protected event EventHandler StartedTouchingTheSides;
    protected event EventHandler StoppedTouchingTheSides;


    protected override void Awake()
    {
        base.Awake();
        SwitchingToCamshift += OnSwitchingToCamshift;
        SwitchingToCascade += OnSwitchingToCascade;
    }

    protected override void Update()
    {
        GetDetectionRects();

        if (!transition.IsRunning())
            UpdateResultRect();

        base.Update();
    }


    void GetDetectionRects()
    {
        if (ShouldUseCamShift)
        {
            // Get camshift rect.
            lastCamshiftRect = new CustomRect(camShift.ResultRect);
            lastCorrectedCamshiftRect = CorrectLeavingRectCenter(lastCamshiftRect);
            if (gotOffsets)
            {
                lastCorrectedCamshiftRect = lastCamshiftRect;
                lastCorrectedCamshiftRect.center += faceDToCamshiftCenterOffset;
                lastCorrectedCamshiftRect.width += faceDToCamshiftWidthOffset;
            }
        }
        else if (cascadeFaceDetection.DetectsAFace)
            // Get faceDetection rect.
            lastFaceDetectionRect = cascadeFaceDetection.Rects[0];
    }

    void UpdateResultRect()
    {
        if (!ShouldUseCamShift && lastFaceDetectionRect != null)
            resultFaceRect = new CustomRect(lastFaceDetectionRect);
        else if (ShouldUseCamShift && CamshiftRectForResult != null)
            resultFaceRect = CamshiftRectForResult;
    }


    #region When switching detection methods ----------------------------
    void OnSwitchingToCamshift()
    {
        GetFaceDCamShiftOffsets();
        StartTransitionToCamshift();
    }

    void OnSwitchingToCascade()
    {
        gotOffsets = false;
        StartTransitionToFaceDetection();
    }

    void GetFaceDCamShiftOffsets()
    {
        faceDToCamshiftCenterOffset = CascadeFaceDetection.CenterRect(lastFaceDetectionRect) - CamshiftRectForResult.center;
        faceDToCamshiftWidthOffset = lastFaceDetectionRect.width - CamshiftRectForResult.width;
        gotOffsets = true;
    }
    #endregion --------------------------------------------------------


    #region Transitions ------------
    void StartTransitionToCamshift()
    {
        // Transition from faceDetection position to camshift position.
        if (transition != null)
        {
            StopCoroutine(transition);
            transition = null;
        }
        transition = this.ProgressionAnim(toCamshiftTransitionDuration, delegate (float progression)
        {
            resultFaceRect = CustomRect.Lerp(new CustomRect(lastFaceDetectionRect), CamshiftRectForResult, progression);
        }, delegate
        {
            transition = null;
        });
    }

    void StartTransitionToFaceDetection()
    {
        // Transition from camshift position to faceDetection position.
        if (transition != null)
        {
            StopCoroutine(transition);
            transition = null;
        }
        transition = this.ProgressionAnim(fromCamshiftTransitionDuration, delegate (float progression)
        {
            resultFaceRect = CustomRect.Lerp(CamshiftRectForResult, new CustomRect(lastFaceDetectionRect), progression);
        }, delegate
        {
            transition = null;
        });
    }
    #endregion -----------------------


    #region Camshift rect correction ------------
    public enum Side { Left, Right, Up, Down }
    List<Side> lastTouchedSides = new List<Side>();
    public List<Side> LastTouchesSidesGetter { get { return lastTouchedSides; } }
    protected CustomRect rectValuesOnTouch;
    float sideTolerance = 10;

    CustomRect CorrectLeavingRectCenter(CustomRect inputRect)
    {
        if (camShift.ResultRect == null)
            return inputRect;

        List<Side> touchedSides = GetTouchedSides(inputRect);

        if (touchedSides.Count > 0)
        {
            // We are touching a side.
            if (lastTouchedSides.Count == 0)
            {
                rectValuesOnTouch = new CustomRect(camShift.ResultRect);
                StartedTouchingTheSides?.Invoke();
            }
            CorrectCenterFromTouchedSides(ref inputRect, touchedSides);
        }
        else
        {
            // We are not touching a side.
            if (lastTouchedSides.Count != 0)
            {
                lastTouchedSides.Clear();
                rectValuesOnTouch = null;
                StoppedTouchingTheSides?.Invoke();
            }
        }

        lastTouchedSides = touchedSides;
        return inputRect;
    }

    List<Side> GetTouchedSides(CustomRect rect)
    {
        // Return a list with all the sides that we are touching.
        List<Side> result = new List<Side>();
        if (rect.Max.x >= RequestedResolution.x - sideTolerance)
            result.Add(Side.Right);
        if (rect.Min.x <= sideTolerance)
            result.Add(Side.Left);
        if (rect.Min.y <= sideTolerance)
            result.Add(Side.Up);
        if (rect.Max.y >= RequestedResolution.y - sideTolerance)
            result.Add(Side.Down);
        return result;
    }

    CustomRect CorrectCenterFromTouchedSides(ref CustomRect inputRect, List<Side> touchedSides)
    {
        // We keep on touching a side, correct the center.
        Point newCenter = inputRect.center;
        float newWidth = inputRect.width;
        float newHeight = inputRect.height;

        foreach (Side touchedSide in touchedSides)
        {
            switch (touchedSide)
            {
                // Offset the center towards the edge, and compensate the width or the height.
                case Side.Left:
                    newCenter.x = inputRect.Max.x - rectValuesOnTouch.width * .5f;
                    newWidth = rectValuesOnTouch.width;
                    break;
                case Side.Right:
                    newCenter.x = inputRect.Min.x + rectValuesOnTouch.width * .5f;
                    newWidth = rectValuesOnTouch.width;
                    break;
                case Side.Up:
                    newCenter.y = inputRect.Max.y - rectValuesOnTouch.height * .5f;
                    newHeight = rectValuesOnTouch.height;
                    break;
                case Side.Down:
                    newCenter.y = inputRect.Min.y + rectValuesOnTouch.height * .5f;
                    newHeight = rectValuesOnTouch.height;
                    break;
                default:
                    break;
            }
        }

        inputRect.center = newCenter;
        inputRect.width = newWidth;
        inputRect.height = newHeight;
        return inputRect;
    }
    #endregion ------------------------------------
}


public class CustomRect
{
    public Point center;
    public float width;
    public float height;
    public Vector2 Min { get { return new Vector2((float)center.x - width * .5f, (float)center.y - height * .5f); } }
    public Vector2 Max { get { return new Vector2((float)center.x + width * .5f, (float)center.y + height * .5f); } }
    public CustomRect(Point _center, float _width, float _height)
    {
        center = _center;
        width = _width;
        height = _height;
    }
    public CustomRect(Rect r)
    {
        center = new Point(r.x + (r.width / 2), r.y + (r.height / 2));
        width = r.width;
        height = r.height;
    }
    public float Area()
    {
        return width * height;
    }
    public static CustomRect Lerp(CustomRect rectA, CustomRect rectB, float t)
    {
        Point center = new Point(Mathf.Lerp((float)rectA.center.x, (float)rectB.center.x, t), Mathf.Lerp((float)rectA.center.y, (float)rectB.center.y, t));
        float width = Mathf.Lerp(rectA.width, rectB.width, t);
        float height = Mathf.Lerp(rectA.height, rectB.height, t);
        return new CustomRect(center, width, height);
    }
}