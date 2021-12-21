using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gets the current pose from the Posenet message handler. 
/// Finds the active pose from the last pose's position.
/// Keeps only the keypoints that are secure enough.
/// When a keypoint is lost, wait a bit defore removing it (this eliminates flickering).
/// </summary>
[RequireComponent(typeof(WebSocketClient), typeof(PosenetMessageHandler))]
public class PosenetPoseTracker : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The minimum confidence score for a pose to be valid.")]
    float poseConfidence = .2f;
    [SerializeField]
    [Tooltip("The minimum confidence score for a keypoint to be valid.")]
    float keypointConfidence = .1f;
    [SerializeField]
    [Tooltip("The delay before a keypoint is declared lost.")]
    float lostDelay = 1;
    /// <summary>
    /// The delay before a keypoint is declared lost.
    /// </summary>
    public float LostDelay { get { return lostDelay; } }

    PosenetMessageHandler messageHandler;
    public PoseNetResult RawResult { get { return messageHandler.Result; } }
    public delegate void EventHandler();
    public event EventHandler PoseUpdated;

    Vector2 lastHeadPosition;
    /// <summary>
    /// All of the detected poses.
    /// </summary>
    public List<PosenetFilteredPose> FilteredPoses { get; private set; } = new List<PosenetFilteredPose>();
    /// <summary>
    /// The current tracked pose (will try to follow the same pose based on its previous position).
    /// </summary>
    public PosenetTrackedPose Pose { get; private set; } = new PosenetTrackedPose();

    public static PosenetPoseTracker Instance { get; private set; }


    private void Awake()
    {
        Instance = this;

        // Update our current pose when a message is received.
        messageHandler = GetComponent<PosenetMessageHandler>();
        messageHandler.MessageReceived += GetCurrentPose;
    }


    private void GetCurrentPose(PoseNetResult result)
    {
        // Filter the poses, then find the current pose.
        FilteredPoses = new List<PosenetFilteredPose>();
        foreach (PoseNetPose pose in result.poses)
        {
            if (pose.score < poseConfidence)
                break;
            FilteredPoses.Add(new PosenetFilteredPose(pose, keypointConfidence));
        }
        if (FilteredPoses.Count > 0)
        {
            KeyValuePair<PosenetFilteredPose, Vector2> closestPoseHead = GetClosestPose(FilteredPoses, lastHeadPosition);
            lastHeadPosition = closestPoseHead.Value;
            Pose.UpdateFromPose(closestPoseHead.Key);
        }
        else
            // No poses found.
            Pose.UpdateFromPose(new PosenetFilteredPose(null, 0));

        // Call event.
        PoseUpdated?.Invoke();
    }

    KeyValuePair<PosenetFilteredPose, Vector2> GetClosestPose(List<PosenetFilteredPose> poses, Vector2 lastPosition)
    {
        List<KeyValuePair<PosenetFilteredPose, Vector2>> posePositions = new List<KeyValuePair<PosenetFilteredPose, Vector2>>();
        // Get each pose's position.
        for (int i = 0; i < poses.Count; i++)
        {
            // Try to get a pose and its head position.
            try
            {
                posePositions.Add(new KeyValuePair<PosenetFilteredPose, Vector2>(poses[i], poses[i].HeadPosition));
            }
            catch { }
        }
        if (posePositions.Count > 0)
            // Return the closest pose and its head position.
            return posePositions.Aggregate((a, b) => Vector2.Distance(lastPosition, a.Value) < Vector2.Distance(lastPosition, b.Value) ? a : b);
        // If no poses were found, return the first one.
        return new KeyValuePair<PosenetFilteredPose, Vector2>(poses[0], lastPosition);
    }
}

public enum PoseKeypointType { nose, leftEye, rightEye, leftEar, rightEar, leftShoulder, rightShoulder, leftElbow, rightElbow, leftWrist, rightWrist, leftHip, rightHip, leftKnee, rightKnee, leftAnkle, rightAnkle, none }


#region Filtered pose, tracked pose -------------
/// <summary>
/// Pose without any keypoints of a low score.
/// </summary>
public class PosenetFilteredPose
{
    public Dictionary<PoseKeypointType, PoseNetKeypoint> Keypoints { get; private set; } = new Dictionary<PoseKeypointType, PoseNetKeypoint>();
    public Vector2 HeadPosition { get { return Utilities.Vector2Center(Keypoints[PoseKeypointType.leftEye].position, Keypoints[PoseKeypointType.rightEye].position); } }

    /// <summary>
    /// </summary>
    /// <param name="inPose">The pose from the parsed result.</param>
    /// <param name="keypointConfidence">The minimum score for a keypoint to be valid.</param>
    public PosenetFilteredPose(PoseNetPose inPose, float keypointConfidence)
    {
        if (inPose == null)
            return;
        foreach (PoseNetKeypoint keypoint in inPose.keypoints)
            if (keypoint.score > keypointConfidence)
                foreach (PoseKeypointType type in (PoseKeypointType[])Enum.GetValues(typeof(PoseKeypointType)))
                    if (keypoint.part.Equals(type.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        Keypoints.Add(type, keypoint);
    }

    public PoseNetKeypoint GetKeypoint(PoseKeypointType keypointType)
    {
        if (Keypoints.ContainsKey(keypointType))
            return Keypoints[keypointType];
        else
            return null;
    }
}

public class PosenetTrackedPose
{
    Dictionary<PoseKeypointType, PoseNetKeypoint> keypoints = new Dictionary<PoseKeypointType, PoseNetKeypoint>();
    Dictionary<PoseKeypointType, float> lostTimes = new Dictionary<PoseKeypointType, float>();

    /// <summary>
    /// Update our keypoints from the keypoints in the pose.
    /// </summary>
    /// <param name="inPose">The pose that only has the tracked keypoints.</param>
    public void UpdateFromPose(PosenetFilteredPose inPose)
    {
        foreach (PoseKeypointType keypointType in (PoseKeypointType[])Enum.GetValues(typeof(PoseKeypointType)))
        {
            if (inPose.Keypoints.ContainsKey(keypointType))
            {
                // Update this keypoint in our collection.
                if (keypoints.ContainsKey(keypointType))
                    keypoints[keypointType] = inPose.Keypoints[keypointType];
                else
                    keypoints.Add(keypointType, inPose.Keypoints[keypointType]);
                // If we have the keypoint in our lost watchList, remove it.
                if (lostTimes.ContainsKey(keypointType))
                    lostTimes.Remove(keypointType);
            }
            else
            {
                // A keypoint is missing. If we don't have it either, continue. 
                if (!keypoints.ContainsKey(keypointType))
                    continue;
                // We are losing this keypoint, check if it is past its lost time.
                if (!lostTimes.ContainsKey(keypointType))
                    lostTimes.Add(keypointType, Time.time + PosenetPoseTracker.Instance.LostDelay);
                if (Time.time >= lostTimes[keypointType])
                {
                    keypoints.Remove(keypointType);
                    lostTimes.Remove(keypointType);
                }
            }
        }
    }

    public PoseNetKeypoint GetKeypoint(PoseKeypointType keypointType)
    {
        if (keypoints.ContainsKey(keypointType))
            return keypoints[keypointType];
        else
            return null;
    }
}
#endregion -------------------------------------