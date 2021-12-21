using System;
using System.Collections.Generic;
using UnityEngine;

public class PosenetVisualizer : MonoBehaviour
{
    List<PoseRenderer> renderers = new List<PoseRenderer>();
    PosenetPoseTracker PoseTracker { get { return PosenetPoseTracker.Instance; } }
    [SerializeField]
    Transform screen;
    [SerializeField]
    Material defaultScreenMat;


    private void Reset()
    {
        if (name == "GameObject")
            this.RenameFromType();
        // Create the screen.
        if (transform.childCount > 0 && transform.GetChild(0).name.Equals("screen", StringComparison.InvariantCultureIgnoreCase))
            screen = transform.GetChild(0);
        else
        {
            screen = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            screen.SetParent(transform);
            screen.localPosition = Vector3.zero.SetZ(.01f);
            screen.name = "Screen";
            if (defaultScreenMat)
                screen.gameObject.GetComponent<Renderer>().material = defaultScreenMat;
        }
    }

    private void Update()
    {
        UpdatePoseRenderers();
        UpdateSize();
    }


    void UpdatePoseRenderers()
    {
        // No pose, hide all and stop.
        if (!PoseTracker || PoseTracker.Pose == null)
        {
            foreach (PoseRenderer rend in renderers)
                rend.GameObject.SetActive(false);
            return;
        }

        // Update the amount of renderers.
        while (renderers.Count < PoseTracker.FilteredPoses.Count)
            renderers.Add(new PoseRenderer(transform));

        // Update each renderer.
        for (int i = 0; i < renderers.Count; i++)
        {
            // Deactivate the renderer if we don't have that many poses, or if the pose is uncertain.
            if (i > PoseTracker.FilteredPoses.Count - 1)
            {
                renderers[i].GameObject.SetActive(false);
                continue;
            }
            // Activate the renderer and update its poses.
            renderers[i].GameObject.SetActive(true);
            renderers[i].UpdateFromPose(PoseTracker.FilteredPoses[i], i == 0 ? Color.white : Color.grey);
        }
    }

    void UpdateSize()
    {
        if (!screen || PoseTracker.RawResult == null)
            return;
        // Update screen scale to fit the image.
        Vector3 landscapeScale = Vector3.one.SetY(PoseTracker.RawResult.image.height / (float)PoseTracker.RawResult.image.width);
        if (PoseTracker.RawResult.image.width > PoseTracker.RawResult.image.height)
            screen.localScale = landscapeScale;
        else
            screen.localScale = Vector3.one.SetX(PoseTracker.RawResult.image.width / (float)PoseTracker.RawResult.image.height);
    }
}


public class PoseRenderer
{
    public GameObject GameObject { get; private set; }

    struct Point
    {
        public GameObject gameObject;
        public Transform Transform { get { return gameObject.transform; } }
        public LineRenderer Rend { get; private set; }
        static readonly float width = .02f;

        public Point(string name, Transform parent)
        {
            gameObject = new GameObject(name);
            gameObject.transform.parent = parent;
            Rend = gameObject.AddComponent<LineRenderer>();
            Rend.useWorldSpace = false;
            Rend.SetPosition(0, new Vector2(-width * .5f, 0));
            Rend.SetPosition(1, new Vector3(width * .5f, 0));
            Rend.startWidth = width;
            Rend.endWidth = width;
            Rend.material = new Material(Shader.Find("Unlit/Color"));
        }
    }
    List<Point> points = new List<Point>();

    public PoseRenderer(Transform parent)
    {
        // Create the points.
        GameObject = new GameObject("new Pose Renderer");
        GameObject.transform.SetParent(parent, false);
        foreach (PoseKeypointType keypointType in (PoseKeypointType[])Enum.GetValues(typeof(PoseKeypointType)))
        {
            if (keypointType == PoseKeypointType.none)
                continue;
            points.Add(new Point(keypointType.ToString(), GameObject.transform));
        }
    }

    /// <summary>
    /// Update the points count, position and color from the pose's keypoints.
    /// </summary>
    /// <param name="pose">Reference pose.</param>
    /// <param name="pointsColor">Points color.</param>
    public void UpdateFromPose(PosenetFilteredPose pose, Color pointsColor)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (!Enum.TryParse(points[i].gameObject.name, out PoseKeypointType type))
                return;
            // Deactivate the point if we don't have it in the pose.
            if (pose.GetKeypoint(type) == null)
            {
                points[i].gameObject.SetActive(false);
                continue;
            }
            // Activate the point and update its position and color. 
            points[i].gameObject.SetActive(true);
            points[i].Transform.localPosition = pose.GetKeypoint(type).RelativePosition();
            points[i].Rend.material.color = pointsColor;
        }
    }
    /// <summary>
    /// Update the points count and position from the pose's keypoints.
    /// </summary>
    /// <param name="pose">Reference pose.</param>
    public void UpdateFromPose(PosenetFilteredPose pose)
    {
        UpdateFromPose(pose, Color.white);
    }
}