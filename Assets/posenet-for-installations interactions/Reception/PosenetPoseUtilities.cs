using UnityEngine;

public static class PosenetPoseUtilities
{
    /// <summary>
    /// Position from -.5 to .5, 0 being at the center.
    /// </summary>
    /// <param name="keypoint"></param>
    /// <param name="image">The image used for the detection.</param>
    /// <returns></returns>
    public static Vector2 RelativePosition(this PoseNetKeypoint keypoint, PoseNetImage image)
    {
        return new Vector2(keypoint.position.x / image.width - .5f, (1 - (keypoint.position.y / image.height) - .5f) * (image.height / (float)image.width));
    }

    /// <summary>
    /// Position from -.5 to .5, relative to the size of the current image in the Posenet result. 0 is at the center.
    /// </summary>
    /// <param name="keypoint"></param>
    /// <returns></returns>
    public static Vector2 RelativePosition(this PoseNetKeypoint keypoint)
    {
        return keypoint.RelativePosition(PosenetPoseTracker.Instance.RawResult.image);
    }
}