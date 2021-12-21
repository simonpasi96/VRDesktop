using UnityEngine;

/// <summary>
/// Checks every frame if the cursor is inactive. If it is inactive for too long, hide it.
/// </summary>
public class CursorMask : MonoBehaviour
{
    Vector3 lastMousePos;
    float lastMoveTime = 0;
    [Tooltip("Time until the cursor will be masked")]
    public float delay = 1.5f;


    private void Reset()
    {
        this.RenameFromType(true);
    }

    private void Update()
    {
        if (Input.mousePosition != lastMousePos)
            lastMoveTime = Time.time;
        lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            // Show the cursor if the user is clicking.
            Cursor.visible = true;
            return;
        }

        if (Time.time - lastMoveTime > delay)
            Cursor.visible = false;
        else
            Cursor.visible = true;
    }
}