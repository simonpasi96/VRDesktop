using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PosenetCursor : MonoBehaviour
{
    [SerializeField]
    RectTransform cursor;
    [SerializeField]
    Image loading;
    Vector3 velocity;
    float cursorVelocityX;
    float cursorVelocityY;

    [Header("Hand area corners")]
    [SerializeField]
    [Tooltip("Upper left corner.\nThe origin is the nose, the distance between each eye is 1 unit.")]
    Vector2 ul = new Vector2(1, 2);
    [SerializeField]
    [Tooltip("Bottom right corner.\nThe origin is the nose, the distance between each eye is 1 unit.")]
    Vector2 br = new Vector2(6.3f, 5);

    PosenetCursor_Button currentButton;
    PosenetTrackedPose CurrentPose { get { return PosenetPoseTracker.Instance.Pose; } }


    private void Reset()
    {
        try
        {
            cursor = (RectTransform)transform.GetChild(0);
            loading = cursor.GetChild(1).gameObject.GetComponent<Image>();
        }
        catch { }
    }

    private void Start()
    {
        transform.SetSiblingIndex(transform.parent.childCount - 1);
        loading.fillAmount = 0;
    }

    private void Update()
    {
        if (PosenetPoseTracker.Instance.Pose == null)
            return;

        // Get keypoints.
        PoseNetKeypoint nose = CurrentPose.GetKeypoint(PoseKeypointType.nose);
        PoseNetKeypoint eye = CurrentPose.GetKeypoint(PoseKeypointType.leftEye);
        PoseNetKeypoint rEye = CurrentPose.GetKeypoint(PoseKeypointType.rightEye);
        PoseNetKeypoint lWrist = CurrentPose.GetKeypoint(PoseKeypointType.leftWrist);
        if (nose == null || eye == null || rEye == null || lWrist == null)
            return;
        // Position.
        Vector2 nosePos = nose.position;
        Vector2 lEyePos = eye.position;
        Vector2 rEyePos = rEye.position;

        // (check if should stay active)
        if (!PoseIsFacingCam(nosePos, lEyePos, rEyePos))
        {
            cursor.gameObject.SetActive(false);
            return;
        }
        else
            cursor.gameObject.SetActive(true);

        float eyeDistance = Vector2.Distance(lEyePos, rEyePos);
        Vector2 areaBRCorner = new Vector2(nosePos.x + eyeDistance * br.x, nosePos.y + eyeDistance * br.y);
        Vector2 areaULCorner = new Vector2(nosePos.x + eyeDistance * ul.x, nosePos.y + eyeDistance * ul.y);

        Vector2 wristPosition = lWrist.position;
        Vector2 cursorRelativePos = new Vector2((wristPosition.x - areaULCorner.x) / (areaBRCorner.x - areaULCorner.x), 1-(wristPosition.y - areaULCorner.y) / (areaBRCorner.y - areaULCorner.y));
        Vector2 cursorPos = cursorRelativePos;
        Rect area = ((RectTransform)transform).rect;
        cursorPos.Scale(new Vector2(area.width, area.height));
        if (cursorRelativePos.magnitude.IsBetween(0, 2))
            cursor.anchoredPosition = Vector3.SmoothDamp(cursor.anchoredPosition, cursorPos, ref velocity, .1f);

        var pointerEventData = new PointerEventData(GetComponentInParent<EventSystem>());
        pointerEventData.position = new Vector2(cursorRelativePos.x * Screen.width, cursorRelativePos.y * Screen.height);
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        GetComponentInParent<GraphicRaycaster>().Raycast(pointerEventData, raycastResults);

        // Clicking.
        bool stillOverButton = false;
        foreach (RaycastResult result in raycastResults)
            if (currentButton != null && result.gameObject.GetComponent<PosenetCursor_Button>() == currentButton)
                stillOverButton = true;
        if (stillOverButton)
            return;
        else
        {
            // Stop highlight and forget last button.
            StopAllCoroutines();
            if (currentButton != null)
            {
                currentButton.OnHoverEnd();
                currentButton = null;
            }
            loading.fillAmount = 0;
        }

        // Try to find a new button to star hover on.
        foreach (RaycastResult result in raycastResults)
        {
            if (!result.gameObject.GetComponent<PosenetCursor_Button>())
                continue;
            currentButton = result.gameObject.GetComponent<PosenetCursor_Button>();
            currentButton.OnHoverStart();
            this.ProgressionAnim(1, delegate (float progression)
            {
                loading.fillAmount = progression;
            }, delegate
            {
                currentButton.Click();
                loading.fillAmount = 0;
            });
        }
    }


    bool PoseIsFacingCam(Vector2 nose, Vector2 lEye, Vector2 rEye)
    {
        float distNoseLEye = Vector2.Distance(nose, lEye);
        float distNoseREye = Vector2.Distance(nose, rEye);
        return distNoseLEye.IsBetween(distNoseREye * .85f, distNoseREye * 1.15f);
    }

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out Point pos);

    public struct Point
    {
        public float x;
        public float y;
    }
}