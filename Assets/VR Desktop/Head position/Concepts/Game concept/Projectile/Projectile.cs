using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5;

    private void Update()
    {
        transform.Translate(-Vector3.forward * Time.deltaTime * speed);
        if (HeadPositionFromFace.Instance && transform.position.z < HeadPositionFromFace.Instance.Position.z - 1)
            Destroy(gameObject);
    }
}
