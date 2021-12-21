using UnityEngine;

public class EchapToSkip : MonoBehaviour {

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Skip();
    }

    public void Skip()
    {
        GetComponentInParent<CalibrationMenu>().Close();
    }
}
