using UnityEngine;
using Rect = OpenCVForUnity.CoreModule.Rect;

/// <summary>
/// Stops the detection if the CAMShift is doing bad stuff.
/// If the CAMShift is too close to the edge of the screen, stop.
/// If, when we switch to the CAMShift, the new center of the face is very far away, it means it wasn't the head that was detected, so stop.
/// </summary>
public class FaceDetectionClamped : FaceDetectionAdvancedTransition
{
    bool faceTouchesTheSides;
    bool lost;
    bool shouldCancelLastCamshiftRect;

    [Space]
    [Tooltip("The detection stops when (outsideDistance > textureSize * boundsThreshold).")]
    [SerializeField]
    float faceLostBoundsThreshold = .05f;
    [Tooltip("The detection stops when the distance between the cascade rect's center and the new camshift rect is greater than texture.height * offsetThreshold.")]
    [SerializeField]
    float faceBadCamshiftOffsetThreshold = .3f;

    Rect lastDetectionRect;
    Rect CurrentRawDetectionRect {
        get {
            if (camShift.canRender)
                return camShift.ResultRect;
            else
                return lastFaceDetectionRect;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        StartedTouchingTheSides += delegate { faceTouchesTheSides = true; };
        StoppedTouchingTheSides += delegate { faceTouchesTheSides = false; };
    }

    protected override void Update()
    {
        // Stop here if the detection is lost.
        if (lost)
        {
            resultFaceRect = null;
            if (!cascadeFaceDetection.DetectsAFace)
                return;
            else
                lost = false;
        }

        // Keep the last detection rect.
        lastDetectionRect = CurrentRawDetectionRect;

        // Get the new face rect.
        base.Update();

        // Check if we got bad tracking info.
        if (camShift.canRender)
            CheckForBadTracking();
    }

    private void LateUpdate()
    {
        // Don't use the last camshift rect if it was bad.
        if (shouldCancelLastCamshiftRect)
        {
            lastCorrectedCamshiftRect = new CustomRect(lastFaceDetectionRect);
            shouldCancelLastCamshiftRect = false;
        }
    }


    void CheckForBadTracking()
    {
        CheckForBadCamshiftOffset();
        // If the face is out of the texture's bounds, check if it lost.
        if (faceTouchesTheSides)
            CheckForFaceLostBounds();
    }

    void CheckForFaceLostBounds()
    {
        // If the face's center is out of the texture's bounds, check if it is too far out.
        float authorizedOffset = RequestedResolution.x * faceLostBoundsThreshold;

        if (!((float)CurrentFaceRect.center.x).IsBetween(-authorizedOffset, RequestedResolution.x + authorizedOffset) || !((float)CurrentFaceRect.center.y).IsBetween(-authorizedOffset, RequestedResolution.y + authorizedOffset))
            lost = true;
    }

    void CheckForBadCamshiftOffset()
    {
        // The camshift tracked the wrong thing if the distance between the previous cascadeFace and the new camshiftResult is too big.
        if (Mathf.Sqrt(Mathf.Pow(lastDetectionRect.x - CurrentRawDetectionRect.x, 2) + Mathf.Pow(lastDetectionRect.y - CurrentRawDetectionRect.y, 2)) > RequestedResolution.y * faceBadCamshiftOffsetThreshold)
        {
            lost = true;
            shouldCancelLastCamshiftRect = true;
        }
    }
}