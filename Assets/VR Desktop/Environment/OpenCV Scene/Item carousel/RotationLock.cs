using UnityEngine;

public class RotationLock : MonoBehaviour {

    Quaternion startRotation;


    private void Awake()
    {
        startRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = startRotation;
    }

    private void OnDestroy()
    {
        transform.rotation = startRotation;
    }
}
