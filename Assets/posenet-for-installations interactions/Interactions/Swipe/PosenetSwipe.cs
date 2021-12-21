using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PosenetSwipe : MonoBehaviour
{
    [Tooltip("The speed at which the hand needs to move to trigger a swipe.")]
    [SerializeField]
    float velocityThreshold = 50;

    #region CanSwipe & swipe events --------------
    public UnityEvent SwipedLeft;
    public UnityEvent SwipedRight;
    public delegate void SwipeEventHandler();
    public SwipeEventHandler InteractionStart;
    public SwipeEventHandler InteractionEnd;
    public SwipeEventHandler LeftStart;
    public SwipeEventHandler LeftEnd;
    public SwipeEventHandler RightStart;
    public SwipeEventHandler RightEnd;
    /// <summary>
    /// Value holder for canSwipe value (don't set directly, use CansSwipe property).
    /// </summary>
    bool cs, csr, csl;
    public bool CanSwipe {
        get { return cs; }
        private set {
            cs = value;
            (value ? InteractionStart : InteractionEnd)?.Invoke();
        }
    }
    public bool CanSwipeRight {
        get { return csr; }
        private set {
            csr = value;
            (value ? RightStart : RightEnd)?.Invoke();
        }
    }
    public bool CanSwipeLeft {
        get { return csl; }
        private set {
            csl = value;
            (value ? LeftStart : LeftEnd)?.Invoke();
        }
    }

    public bool IsLeftReady { get { return CanSwipeLeft && LWrist.position.x > LElbow.position.x + LArmLength * .5f; } }
    public bool IsRightReady { get { return CanSwipeRight && RWrist.position.x < RElbow.position.x + RArmLength * .5f; } }
    #endregion ----------------------------

    #region Pose keypoints --------
    PosenetTrackedPose CurrentPose { get { return PosenetPoseTracker.Instance.Pose; } }
    PoseNetKeypoint LShoulder { get { return CurrentPose.GetKeypoint(PoseKeypointType.leftShoulder); } }
    PoseNetKeypoint RShoulder { get { return CurrentPose.GetKeypoint(PoseKeypointType.rightShoulder); } }

    PoseNetKeypoint LElbow { get { return CurrentPose.GetKeypoint(PoseKeypointType.leftElbow); } }
    PoseNetKeypoint LWrist { get { return CurrentPose.GetKeypoint(PoseKeypointType.leftWrist); } }

    PoseNetKeypoint RElbow { get { return CurrentPose.GetKeypoint(PoseKeypointType.rightElbow); } }
    PoseNetKeypoint RWrist { get { return CurrentPose.GetKeypoint(PoseKeypointType.rightWrist); } }
    #endregion --------------------

    #region Wrist min-max position for swiping -------------
    float LArmLength { get { return Vector2.Distance(LShoulder.position, LElbow.position); } }
    float RArmLength { get { return Vector2.Distance(RShoulder.position, RElbow.position); } }
    Vector2 LWristSwipeMinMax {
        get {
            // Min is the shoulder, max is just under the elbow.
            return new Vector2(LShoulder.position.y, LElbow.position.y + LArmLength * .3f);
        }
    }
    Vector2 RWristSwipeMinMax {
        get {
            // Min is the shoulder, max is just under the elbow.
            return new Vector2(RShoulder.position.y, RElbow.position.y + RArmLength * .3f);
        }
    }
    #endregion ---------------------------------------------

    List<float> lastLWristXPos = new List<float>();
    List<float> lastRWristXPos = new List<float>();
    bool canUpdate = false;


    private void Start()
    {
        PosenetPoseTracker.Instance.PoseUpdated += UpdateSwipeDetection;
    }


    private void UpdateSwipeDetection()
    {
        if (PosenetPoseTracker.Instance.Pose == null)
            return;

        // Get tracking state and whether or not we can swipe.
        bool isLeftTracked = LShoulder != null && LElbow != null && LWrist != null;
        bool isRightTracked = RShoulder != null && RElbow != null && RWrist != null;

        // Can swipe if tracked and in-between the shoulder and the elbow.
        CanSwipeLeft = isLeftTracked && LWrist.position.y.IsBetween(LWristSwipeMinMax.x, LWristSwipeMinMax.y);
        CanSwipeRight = isRightTracked && RWrist.position.y.IsBetween(RWristSwipeMinMax.x, RWristSwipeMinMax.y);

        // If can swipe, check for swipes.
        if (CanSwipeLeft)
            DetectLeftSwipe();
        if (CanSwipeRight)
            DetectRightSwipe();

        // Call events when the interaction is starting or is ending.
        if (CanSwipe)
        {
            if ((!CanSwipeRight) && (!CanSwipeLeft))
            {
                CanSwipe = false;
                OnInteractionEnd();
            }
        }
        else
        {
            if (CanSwipeRight || CanSwipeLeft)
            {
                CanSwipe = true;
                OnInteractionStart();
            }
        }
    }

    void DetectLeftSwipe()
    {
        Vector2 newWristPos = LWrist.position;
        if (lastLWristXPos.Count > 3)
        {
            float handVelocity = newWristPos.x - lastLWristXPos[0];
            // If the hand is moving fast and passed in front of elbow, swipe.
            if (handVelocity < -velocityThreshold
                && newWristPos.x < LElbow.position.x && lastLWristXPos[lastLWristXPos.Count - 1] > LElbow.position.x)
                OnSwipeLeft();
        }
        // Remember the new hand position.
        lastLWristXPos.Add(newWristPos.x);
        if (lastLWristXPos.Count > 4)
            lastLWristXPos.RemoveAt(0);
    }

    void DetectRightSwipe()
    {
        Vector2 newWristPos = RWrist.position;
        if (lastRWristXPos.Count > 3)
        {
            float handVelocity = newWristPos.x - lastRWristXPos[0];
            // If the hand is moving fast and passed in front of elbow, swipe.
            if (handVelocity > velocityThreshold
                && newWristPos.x > RElbow.position.x && lastRWristXPos[lastRWristXPos.Count - 1] < RElbow.position.x)
                OnSwipeRight();
        }
        // Remember the new hand position.
        lastRWristXPos.Add(newWristPos.x);
        if (lastRWristXPos.Count > 4)
            lastRWristXPos.RemoveAt(0);
    }


    #region Calling events ----------
    void OnSwipeLeft()
    {
        print("LEFT");
        SwipedLeft.Invoke();
    }

    void OnSwipeRight()
    {
        print("RIGHT");
        SwipedRight.Invoke();
    }

    void OnInteractionEnd()
    {
        lastLWristXPos.Clear();
        lastRWristXPos.Clear();
        InteractionEnd?.Invoke();
    }

    void OnInteractionStart()
    {
        InteractionStart?.Invoke();
    }
    #endregion --------------------
}