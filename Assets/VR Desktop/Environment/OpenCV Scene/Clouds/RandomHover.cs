using UnityEngine;

public class RandomHover : MonoBehaviour
{
    Vector3 startPosition;
    Vector2 randomLoopOffset;

    [SerializeField]
    Vector2 amplitude = new Vector2(.01f, .01f);
    [SerializeField]
    float loopTime = 5;


    private void Awake()
    {
        startPosition = transform.localPosition;
        randomLoopOffset = new Vector2(Random.value * loopTime, Random.value * loopTime);
    }


    private void Update()
    {
        float newX = startPosition.x + Mathf.Sin(Time.time / loopTime + randomLoopOffset.x) * amplitude.x;
        float newZ = startPosition.z + Mathf.Sin(Time.time / loopTime + randomLoopOffset.y) * amplitude.y;

        transform.localPosition = new Vector3(newX, startPosition.y, newZ);
    }
}
