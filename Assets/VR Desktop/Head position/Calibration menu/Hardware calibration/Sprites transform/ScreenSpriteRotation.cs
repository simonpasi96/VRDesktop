using UnityEngine;

public class ScreenSpriteRotation : MonoBehaviour {
    private void Awake()
    {
        UpdateRotation();
        ScreenRotationInput.Changed += UpdateRotation;
    }

    void UpdateRotation()
    {
        transform.localEulerAngles = transform.localEulerAngles.SetZ(ScreenRotationInput.Rotation);
    }
}
