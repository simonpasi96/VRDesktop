using UnityEngine;

public class FOVSpriteScale : MonoBehaviour {


    private void Awake()
    {
        UpdateSize();
        WebcamFOVInput.Changed += UpdateSize;
    }


    void UpdateSize()
    {
        float newScale = Mathf.Tan(WebcamFOVInput.FOV * .5f * Mathf.Deg2Rad) * 2;
        transform.localScale = new Vector3(1, newScale, 1);
    }
}
