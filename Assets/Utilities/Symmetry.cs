using UnityEngine;

[ExecuteInEditMode]
public class Symmetry : MonoBehaviour
{
    public Transform a, b;
    public static bool creating = false;
    Vector3 previousPos;
    Vector3 previousRot;
    Vector3 previousScale;
    [SerializeField]
    bool updateOnPlay = false;


    private void Reset()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorUtility.DisplayDialog("Symmetry setup", "Create a symmetry parent?", "Yes", "No, only add the component"))
            return;
#endif

        if (!creating)
            CreateSymObject();
        else
            creating = false;
    }

    private void Awake()
    {
        if (!updateOnPlay && Application.isPlaying)
            enabled = false;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (a && b)
        {
            CheckPositionChange(a, b);
            CheckPositionChange(b, a);
            UpdatePrevious();
        }
    }
#endif


    public void CreateSymObject()
    {
        creating = true;
        GameObject symGO = new GameObject();
        symGO.name = name + " Symmetry";
        symGO.transform.parent = transform.parent;
        transform.parent = symGO.transform;

        Symmetry sym = symGO.AddComponent<Symmetry>();
        sym.a = transform;
        DestroyImmediate(this);

#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = symGO;
#endif
    }

    void CheckPositionChange(Transform symObject, Transform otherSymObject)
    {
        if (symObject.position != previousPos)
            otherSymObject.position = symObject.position.SetX(-symObject.position.x);
        if (symObject.localEulerAngles != previousRot)
            otherSymObject.localEulerAngles = symObject.localEulerAngles.SetY(-symObject.localEulerAngles.y);
        if (symObject.localScale != previousScale)
            otherSymObject.localScale = symObject.localScale;
    }

    void UpdatePrevious()
    {
        previousPos = a.position;
        previousRot = a.localEulerAngles;
        previousScale = a.localScale;
    }
}