using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ScreenRotationInput : MonoBehaviour {
    [SerializeField]
    InputField inputField;
    Text placeholder;

    const string Key = "ScreenRot";
    /// <summary>
    /// Rotation of the screen in degrees.
    /// </summary>
    public static float Rotation {
        get {
            if (!PlayerPrefs.HasKey(Key))
                PlayerPrefs.SetFloat(Key, -10);
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
        // Setup placeholder.
        placeholder = inputField.placeholder.GetComponent<Text>();

        // Listen to input field.
        inputField.onValueChanged.AddListener(OnRotationInput);
    }

    private void OnEnable()
    {
        // Update placeholder and start value.
        startRotation = Rotation;
        placeholder.text = startRotation.ToString();
    }


    void OnRotationInput(string input)
    {
        if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float newRot))
            Rotation = newRot;
        else
            Rotation = startRotation;
        Changed?.Invoke();
    }
}
