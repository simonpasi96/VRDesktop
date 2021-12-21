using UnityEngine;

public class PosenetPointer : MonoBehaviour
{
    PosenetPointingSelectable[] selectables;
    public PosenetPointingSelectable CurrentSelectable { get { return selectables[previousSelection < 0 ? 0 : previousSelection]; } }
    int previousSelection = -1;
    int selectedIndex = 0;

    #region Keypoints -------------
    PosenetTrackedPose CurrentPose { get { return PosenetPoseTracker.Instance.Pose; } }
    PoseNetKeypoint CurrentLShoulder { get { return CurrentPose.GetKeypoint(PoseKeypointType.leftShoulder); } }
    PoseNetKeypoint CurrentLElbow { get { return CurrentPose.GetKeypoint(PoseKeypointType.leftElbow); } }
    PoseNetKeypoint CurrentLWrist { get { return CurrentPose.GetKeypoint(PoseKeypointType.leftWrist); } }
    PoseNetKeypoint CurrentRShoulder { get { return CurrentPose.GetKeypoint(PoseKeypointType.rightShoulder); } }
    PoseNetKeypoint CurrentRElbow { get { return CurrentPose.GetKeypoint(PoseKeypointType.rightElbow); } }
    PoseNetKeypoint CurrentRWrist { get { return CurrentPose.GetKeypoint(PoseKeypointType.rightWrist); } }
    #endregion ---------------------

    [SerializeField]
    Vector2 minMaxAngle = new Vector2(45, 135);
    bool usingLeftArm = true;

    [SerializeField]
    float lostTrackingDelay = .1f;
    float lastLostTime;
    bool lost = false;

    [SerializeField]
    [Tooltip("The time for which the hand needs to be maintained to trigger a click.")]
    float clickDuration = 1;
    bool isSelecting;
    public float LoadingProgression { get; private set; }
    #region Selection events -----------
    public delegate void PointingEventHandler(PosenetPointer caller);
    public static event PointingEventHandler SelectionStart;
    public static event PointingEventHandler NewHover;
    public static event PointingEventHandler Loading;
    public static event PointingEventHandler Selected;
    public static event PointingEventHandler SelectionEnd;
    #endregion -------------------------


    private void Awake()
    {
        selectables = GetComponentsInChildren<PosenetPointingSelectable>();
    }

    private void Start()
    {
        selectables[selectedIndex].Select();
    }

    private void Update()
    {
        if (PosenetPoseTracker.Instance.Pose == null)
            return;

        int currentSelection = GetCurrentSelection();

        if (currentSelection < 0)
            OnNotSelecting();
        else
            OnSelecting(currentSelection);
    }

    private void OnDisable()
    {
        OnNotSelecting();
    }


    #region Get selection from arm angle ----------
    int GetCurrentSelection()
    {
        int newSelection;
        // Get the selection from the current forearm's angle.
        if (usingLeftArm)
            newSelection = GetLArmSelection();
        else
            newSelection = GetRArmSelection();

        // If we found nothing, try the other arm.
        if (newSelection < 0)
        {
            if (usingLeftArm)
                newSelection = GetRArmSelection();
            else
                newSelection = GetLArmSelection();

            // If we found something, try the other arm.
            if (newSelection >= 0)
                usingLeftArm = !usingLeftArm;
        }
        return newSelection;
    }

    int GetLArmSelection()
    {
        return GetArmSelection(CurrentLShoulder, CurrentLElbow, CurrentLWrist);
    }

    int GetRArmSelection()
    {
        return GetArmSelection(CurrentRShoulder, CurrentRElbow, CurrentRWrist);
    }

    int GetArmSelection(PoseNetKeypoint shoulder, PoseNetKeypoint elbow, PoseNetKeypoint wrist)
    {
        if (PosenetPoseTracker.Instance.Pose == null)
            return -1;
        // Dont track if any keypoint isn't here, or if the wrist is beneath the shoulder.
        if (shoulder == null || elbow == null || wrist == null || wrist.position.y > shoulder.position.y)
        {
            // Wait a little bit of time before losing the tracking.
            if (!lost)
            {
                lost = true;
                lastLostTime = Time.time;
            }
            if (Time.time < lastLostTime + lostTrackingDelay)
                return previousSelection;
            // We are not tracking anymore, quit.
            return -1;
        }
        else
            lost = false;

        // Get forearm angle.
        Vector2 forearm = wrist.RelativePosition() - elbow.RelativePosition();
        float armAngle = Vector2.Angle(forearm, Vector2.left);

        // Get a selection from the arm angle.
        float angleProgression = Mathf.Clamp01((armAngle - minMaxAngle.x) / (minMaxAngle.y - minMaxAngle.x));
        return Utilities.IndexFromProgression(angleProgression, selectables.Length);
    }
    #endregion --------------------

    void OnSelecting(int currentSelection)
    {
        isSelecting = true;

        // Update the current selectable.
        if (previousSelection != currentSelection)
            OnNewHover(currentSelection);
    }

    void OnNotSelecting()
    {
        if (!isSelecting)
            return;
        isSelecting = false;

        // Stop the selection.
        StopAllCoroutines();
        previousSelection = -1;
        SelectionEnd?.Invoke(this);
    }

    void OnNewHover(int selection)
    {
        int prevSelHolder = previousSelection;
        previousSelection = selection;
        // Starting the selection, or getting a new hover.
        if (prevSelHolder == -1)
            SelectionStart?.Invoke(this);
        else
            NewHover?.Invoke(this);
        StopAllCoroutines();
        // If the selection is already selected, stop.
        if (selection == selectedIndex)
            return;
        // Start selecting the selectable.
        this.ProgressionAnim(clickDuration, delegate (float progression)
        {
            LoadingProgression = progression;
            Loading?.Invoke(this);
        }, delegate
        {
            // Validate the selection.
            selectables[selectedIndex].Deselect();
            selectedIndex = selection;
            selectables[selectedIndex].Select();
            Selected?.Invoke(this);
        });
    }
}