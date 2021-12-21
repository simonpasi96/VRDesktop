using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Allows a swipe with only one arm. 
/// Once you started swiping in one direction, you can keep swiping in that direction. You have to wait for a little bit until you can swipe in the other direction.
/// If you lower your arm, this delay for swiping the other way is removed.
/// </summary>
public class PosenetSwipeSingleArm : MonoBehaviour
{
    [Tooltip("The speed at which the hand needs to move to trigger a swipe.")]
    [SerializeField]
    float velocityThreshold = 30;

    bool canSwipeRight, canSwipeLeft;

    public UnityEvent SwipedRight;
    public UnityEvent SwipedLeft;
    public UnityEvent InteractionEnd;

    PoseNetKeypoint LShoulder { get { return PosenetPoseTracker.Instance.Pose.GetKeypoint(PoseKeypointType.leftShoulder); } }
    PoseNetKeypoint LElbow { get { return PosenetPoseTracker.Instance.Pose.GetKeypoint(PoseKeypointType.leftElbow); } }
    PoseNetKeypoint LWrist { get { return PosenetPoseTracker.Instance.Pose.GetKeypoint(PoseKeypointType.leftWrist); } }
    List<float> lastLWristXPositions = new List<float>();


    private void Start()
    {
        PosenetPoseTracker.Instance.PoseUpdated += UpdateSwipeDetection;
    }


    private void UpdateSwipeDetection()
    {
        if (PosenetPoseTracker.Instance.Pose == null)
            return;

        // Update confidence and tracking state.
        bool isTracking = LShoulder != null && LElbow != null && LWrist != null;
        if (!isTracking)
        {
            lastLWristXPositions.Clear();
            return;
        }

        // Get the current wrist x position.
        lastLWristXPositions.Add(LWrist.position.x);
        if (lastLWristXPositions.Count > 3)
            lastLWristXPositions.RemoveAt(0);

        // Stop if the hand is too low or too high.
        if (!LWrist.position.y.IsBetween(LShoulder.position.y, LElbow.position.y))
        {
            if (!canSwipeLeft || !canSwipeRight)
            {
                canSwipeLeft = canSwipeRight = true;
                OnInteractionEnd();
            }
            return;
        }

        // If the hand is moving fast and passed in front of elbow, swipe.
        float handVelocity = LWrist.position.x - lastLWristXPositions[0];
        if (canSwipeRight && handVelocity < -velocityThreshold
            && lastLWristXPositions[lastLWristXPositions.Count - 1] < LElbow.position.x && lastLWristXPositions[0] > LElbow.position.x)
            OnSwipeLeft();
        else if (canSwipeLeft && handVelocity > velocityThreshold
            && lastLWristXPositions[lastLWristXPositions.Count - 1] > LElbow.position.x && lastLWristXPositions[0] < LElbow.position.x)
            OnSwipeRight();
    }

    void OnSwipeLeft()
    {
        print("LEFT");
        SwipedLeft.Invoke();
        canSwipeRight = false;
    }

    void OnSwipeRight()
    {
        print("RIGHT");
        SwipedRight.Invoke();
        canSwipeLeft = false;
    }

    void OnInteractionEnd()
    {
        print("Swipe interaction end.");
        InteractionEnd.Invoke();
    }
}