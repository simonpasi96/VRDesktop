using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PosenetDial : MonoBehaviour
{
    [SerializeField]
    Image a, b, c, d;
    List<Image> buttons = new List<Image>();

    PoseNetKeypoint LElbow { get { return PosenetPoseTracker.Instance.Pose.GetKeypoint(PoseKeypointType.leftElbow); } }
    PoseNetKeypoint LWrist { get { return PosenetPoseTracker.Instance.Pose.GetKeypoint(PoseKeypointType.leftWrist); } }


    private void Reset()
    {
        try
        {
            a = transform.GetChild(0).gameObject.GetComponent<Image>();
            b = transform.GetChild(1).gameObject.GetComponent<Image>();
            c = transform.GetChild(2).gameObject.GetComponent<Image>();
            d = transform.GetChild(3).gameObject.GetComponent<Image>();
        }
        catch { }
    }

    private void Start()
    {
        RemoveAllHighlights();
    }

    private void Update()
    {
        if (PosenetPoseTracker.Instance.Pose == null)
            return;

        if (LElbow == null || LWrist== null)
            return;

        Vector2 lForearm = LWrist.RelativePosition() - LElbow.RelativePosition();

        foreach (Image button in buttons)
            if (Vector2.Angle(lForearm, button.transform.localPosition) < 45)
                button.color = Color.white;
            else
                button.color = Color.grey;
    }


    void RemoveAllHighlights()
    {
        buttons = new List<Image>() { a, b, c, d };
        foreach (Image button in buttons)
            button.color = Color.grey;
    }
}