using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class WebcamFOVInput : MonoBehaviour {

    [SerializeField]
    InputField inputField;
    Text placeholder;

    const string key = "FOV";
    public static float FOV {
        get {
            if (!PlayerPrefs.HasKey(key))
                PlayerPrefs.SetFloat(key, 60);
            return PlayerPrefs.GetFloat(key);
        }
        set {
            PlayerPrefs.SetFloat(key, value);
            // Tell the head position about this change.
            if (HeadPositionFromFace.Instance)
                HeadPositionFromFace.Instance.OnFOVChange();
        }
    }
    float startFOV;

    public delegate void EventHandler();
    public static event EventHandler Changed;

    private void Awake()
    {
        // Setut placeholder.
        placeholder = inputField.placeholder.GetComponent<Text>();

        // Listen to input field.
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnEnable()
    {
        // Update placeholder and start values.
        startFOV = FOV;
        placeholder.text = startFOV.ToString();
    }

    void OnValueChanged(string input)
    {
        // Use the new input or the start value.
        if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float newRot))
            FOV = newRot;
        else
            FOV = startFOV;
        Changed?.Invoke();
    }
}
