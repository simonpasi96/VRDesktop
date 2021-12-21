using UnityEngine;

public class ParentOnAwake : MonoBehaviour
{
    [SerializeField]
    Transform parent;

    private void Awake()
    {
        if (GetComponent<RectTransform>() != null && parent.GetComponent<RectTransform>() == null)
            return;
        transform.parent = parent;
    }
}