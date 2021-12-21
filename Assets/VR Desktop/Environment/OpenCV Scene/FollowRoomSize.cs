using UnityEngine;

public class FollowRoomSize : MonoBehaviour {
    [SerializeField]
    Transform room;

    private void Update()
    {
        transform.localScale = Vector3.one * room.lossyScale.x;
    }

}
