using UnityEngine;

public class ItemCarousel : MonoBehaviour
{
    [SerializeField]
    Transform pivot;
    [SerializeField]
    Transform[] stands;
    [SerializeField]
    float rotationDuration = .5f;

    [SerializeField]
    GameObject prevButton, nextButton;

    int currentStand = 0;
    bool isRotating = false;

    float RotationIncrement { get { return 360 / (float)stands.Length; } }

    public Transform CurrentStand { get { return stands[currentStand]; } }
    public Transform CurrentItem { get { return stands[currentStand].GetChild(0); } }

    public void Next()
    {
        if (isRotating)
            return;
        SelectNextOrPrevious(true);
    }

    public void Previous()
    {
        if (isRotating)
            return;
        SelectNextOrPrevious(false);
    }

    void SelectNextOrPrevious(bool next)
    {
        isRotating = true;

        // Deactivate prev-next buttons.
        prevButton.SetActive(false);
        nextButton.SetActive(false);

        // Get the new item index.
        int newItemIndex = currentStand + (next ? 1 : -1);
        if (newItemIndex > stands.Length - 1)
            newItemIndex = 0;
        else if (newItemIndex < 0)
            newItemIndex = stands.Length - 1;

        // Get start rotation and target rotation.
        Vector3 startRotation = pivot.localEulerAngles;
        Vector3 targetRotation = pivot.localEulerAngles.SetY(pivot.localEulerAngles.y + (next ? 1 : -1) * RotationIncrement);
        // Lock each item's rotation.
        foreach (Transform stand in stands)
            stand.GetOrAddComponent<RotationLock>();

        // Rotate and change the scale of the items over time.
        this.ProgressionAnim(rotationDuration, delegate (float progression)
        {
            pivot.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, progression);
        }, delegate
        {
            isRotating = false;
            currentStand = newItemIndex;
            prevButton.SetActive(true);
            nextButton.SetActive(true);
            // Destroy all rotation locks.
            RotationLock[] rotationLocks = pivot.GetComponentsInChildren<RotationLock>();
            for (int i = rotationLocks.Length - 1; i >= 0; i--)
                Destroy(rotationLocks[i]);
        });
    }

    #region Posenet swipe Highlight tests ------------
    private void Start()
    {
        HighlightOff();
    }

    public void HighlightOn()
    {
        prevButton.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        nextButton.GetComponent<UnityEngine.UI.Image>().color = Color.white;
    }
    public void HighlightOff()
    {
        prevButton.GetComponent<UnityEngine.UI.Image>().color = Color.grey;
        nextButton.GetComponent<UnityEngine.UI.Image>().color = Color.grey;
    }
    #endregion ---------------------------------------
}
