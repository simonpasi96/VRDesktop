using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class WebcamRotationInput : MonoBehaviour {

    [SerializeField]
    InputField inputField;
    Text placeholder;

    const string Key = "WebcamRot";
    /// <summary>
    /// Rotation of the webcam in degrees.
    /// </summary>
    public static float Rotation {
        get {
            // Default rotation.
            if (!PlayerPrefs.HasKey(Key))
                PlayerPrefs.SetFloat(Key, 10);
            return PlayerPrefs.GetFloat(Key);
        }
        set {
            PlayerPrefs.SetFloat(Key, value);
        }
    }
    float startRotation;

    public delegate void EventHandler();
    public static event EventHandler Changed;


    private void Awake()
    {
        placeholder = inputField.placeholder.GetComponent<Text>();

        // Listen to input field.
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnEnable()
    {
        // Update start rotation and palceholder.
        startRotation = Rotation;
        placeholder.text = startRotation.ToString();
    }


    void OnValueChanged(string input)
    {
        // Use the new input or the start value.
        if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float newRot))
            Rotation = newRot;
        else
            Rotation = startRotation;
        Changed?.Invoke();
    }
}
