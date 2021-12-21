using UnityEngine;
using UnityEngine.PostProcessing;

public class MaintainPPFocusPoint : MonoBehaviour {
    float startDistance;
    [SerializeField]
        float startFocusDistance = .46f;
    PostProcessingProfile ppProfile;

    private void Awake()
    {
        startDistance = transform.position.z;
        ppProfile = GetComponent<PostProcessingBehaviour>().profile;
        startFocusDistance = ppProfile.depthOfField.settings.focusDistance;
    }

    void Update()
    {
        // Update focus distance to keep it at the same point in space.
        SetFocusDistance(startFocusDistance - (transform.position.z - startDistance));
    }

    private void SetFocusDistance(float newDistance)
    {
        print(transform.position.z - startDistance);
        print(newDistance + " new distance");
        print("start foc " + startFocusDistance);
        DepthOfFieldModel.Settings newSettings = ppProfile.depthOfField.settings;
        newSettings.focusDistance = newDistance;
        ppProfile.depthOfField.settings = newSettings;
    }

    private void OnDestroy()
    {
        SetFocusDistance(startFocusDistance);
    }
}
