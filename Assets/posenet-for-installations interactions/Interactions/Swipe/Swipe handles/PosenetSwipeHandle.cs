using UnityEngine;

public class PosenetSwipeHandle : MonoBehaviour
{
    [SerializeField]
    PosenetSwipe swipe;
    [SerializeField]
    ParticleSystem leftHandle, rightHandle;


    private void Reset()
    {
        swipe = FindObjectOfType<PosenetSwipe>();
        if (transform.childCount > 1)
        {
            leftHandle = GetComponentInChildren<ParticleSystem>();
            rightHandle = GetComponentsInChildren<ParticleSystem>()[1];
        }
    }

    private void Start()
    {
        if (!leftHandle || !rightHandle)
            return;
        swipe.LeftStart += delegate { leftHandle.gameObject.SetActive(true); };
        swipe.LeftEnd += delegate { leftHandle.gameObject.SetActive(false); };
        swipe.SwipedLeft.AddListener(delegate { leftHandle.Play(); });
        swipe.RightStart += delegate { rightHandle.gameObject.SetActive(true); };
        swipe.RightEnd += delegate { rightHandle.gameObject.SetActive(false); };
        swipe.SwipedRight.AddListener(delegate { rightHandle.Play(); });
    }
}