using UnityEngine;

public class FogPositionKeeper : MonoBehaviour {

    [SerializeField]
    Camera camToFollow;
    float cameraStartPosition;
    /// <summary>
    /// x: startDistance, y: endDistance.
    /// </summary>
    Vector2 fogDistanceAtStart;

    float CameraPosition { get { return CameraFromHeadPosition.Instance.transform.position.z; } }


    private void Start()
    {
        if (camToFollow == null)
            camToFollow = Camera.main;
        cameraStartPosition = CameraPosition;
        fogDistanceAtStart = new Vector2(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);
    }

    private void LateUpdate()
    {
        RenderSettings.fogStartDistance = fogDistanceAtStart.x + (cameraStartPosition - CameraPosition);
        RenderSettings.fogEndDistance = fogDistanceAtStart.y + (cameraStartPosition - CameraPosition);
    }
}
