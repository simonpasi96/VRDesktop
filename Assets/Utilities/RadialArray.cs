using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class RadialArray : MonoBehaviour
{
    [SerializeField]
    GameObject template;
    [SerializeField]
    int amount = 2;
    [SerializeField]
    [Tooltip("Minimum amount of imaginary balls to spawn, determines the space between each spawned object.")]
    int minIncrements = 8;
    [SerializeField]
    float radius = 1;
    [SerializeField]
    bool updateInPlayMode = false;
    [Header("Navigation")]
    [SerializeField]
    bool smoothTransition = true;
    [SerializeField]
    float transitionSpeed = 400;
    [SerializeField]
    bool reverseAtTheEnd = true;
    int currentIndex = 0;
    Quaternion startRotation;
    float IncrementAngle { get { return 360f / Mathf.Max(amount, minIncrements); } }


    private void Reset()
    {
        if (name == "GameObject")
            name = "Radial array";
        transform.DestroyChidren();
    }

    private void Awake()
    {
        startRotation = transform.rotation;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (!updateInPlayMode)
                return;
        UpdateObjectCount();
    }


    void UpdateObjectCount()
    {
        if (!template)
            return;

        if (transform.childCount < amount)
            for (int i = transform.childCount; i < amount; i++)
                Instantiate(template, transform);
        else if (transform.childCount > amount && amount >= 0)
            for (int i = transform.childCount; i > amount; i--)
                DestroyImmediate(transform.GetChild(0).gameObject);
        // Update each child's position.
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).localPosition = PointOnCircle(radius, i / (float)Mathf.Max(amount, minIncrements) * 360);
    }

    Vector2 PointOnCircle(float radius, float angle)
    {
        Vector2 pos;
        pos.x = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        return pos;
    }

    public void Next()
    {
        if (smoothTransition)
        {
            // Rotate the array over time.
            currentIndex++;
            if (currentIndex < amount)
                StartCoroutine(Rotate(IncrementAngle, transitionSpeed));
            else
            {
                currentIndex = 0;
                StartCoroutine(Rotate(reverseAtTheEnd ? -IncrementAngle * (amount - 1) : IncrementAngle * (amount < minIncrements ? minIncrements - amount + 1 : 1), transitionSpeed * 2));
            }
            return;
        }

        // Rotate the array instantly.
        Utilities.LimitedIncrement(ref currentIndex, transform.childCount);
        if (currentIndex == 0)
            transform.rotation = startRotation;
        else
            transform.Rotate(Vector3.forward, IncrementAngle, Space.Self);
    }

    public void Previous()
    {
        if (smoothTransition)
        {
            // Rotate the array over time.
            currentIndex--;
            if (currentIndex >= 0)
                StartCoroutine(Rotate(-IncrementAngle, transitionSpeed));
            else
            {
                currentIndex = amount - 1;
                StartCoroutine(Rotate(reverseAtTheEnd ? IncrementAngle * (amount - 1) : -IncrementAngle * (amount < minIncrements ? minIncrements - amount + 1 : 1), transitionSpeed * 2));
            }
            return;
        }

        // Rotate the array instantly.
        Utilities.LimitedDecrement(ref currentIndex, transform.childCount);
        if (currentIndex == transform.childCount - 1)
            transform.Rotate(Vector3.forward, IncrementAngle * (transform.childCount - 1), Space.Self);
        else
            transform.Rotate(Vector3.forward, -IncrementAngle, Space.Self);
    }

    IEnumerator Rotate(float angle, float speed)
    {
        float rotatedAmount = 0;
        while ((angle > 0 && rotatedAmount < angle) || (angle < 0 && rotatedAmount > angle))
        {
            float newRotAngle = (angle > 0 ? 1 : -1) * Time.deltaTime * speed;
            // Clamp the rotation angle to not rotate too much.
            if (angle > 0)
            {
                if (angle - newRotAngle < rotatedAmount)
                    newRotAngle = angle - rotatedAmount;
            }
            else if (angle - newRotAngle > rotatedAmount)
                newRotAngle = angle - rotatedAmount;
            // Rotate.
            transform.Rotate(Vector3.forward, newRotAngle, Space.Self);
            rotatedAmount += newRotAngle;
            yield return null;
        }
    }

    [ContextMenu("Respawn")]
    void EditorUtilityRespawn()
    {
        transform.DestroyChidren();
    }
}