using UnityEngine;
using UnityEngine.UI;

public class RoomLengthChanger : MonoBehaviour {
    float startScale;

    public delegate void EventHandler(RoomLengthChanger sender);
    public event EventHandler Changed;

    public float ScaleFactor { get; private set; }

    private void Awake()
    {
        startScale = transform.localScale.z;
        ScaleFactor = 1;
    }

    public void SetScaleFromField(InputField input)
    {
        ScaleFactor = float.Parse(input.text);
        transform.localScale = transform.localScale.SetZ(startScale * ScaleFactor);
        Changed?.Invoke(this);
    }
}
