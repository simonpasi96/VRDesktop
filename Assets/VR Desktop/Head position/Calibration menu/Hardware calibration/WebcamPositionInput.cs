using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class WebcamPositionInput : MonoBehaviour {

    [SerializeField]
    InputField xInput, yInput, zInput;
    Text placeHolderX, placeHolderY, placeHolderZ;

    const string XKey = "XPos";
    /// <summary>
    /// X position of the webcam in cm.
    /// </summary>
    public static float X {
        get {
            if (!PlayerPrefs.HasKey(XKey))
                PlayerPrefs.SetFloat(XKey, -3);
            return PlayerPrefs.GetFloat(XKey);
        }
        set {
            PlayerPrefs.SetFloat(XKey, value);
        }
    }
    float startX;

    const string YKey = "YPos";
    /// <summary>
    /// Y position of the webcam in cm.
    /// </summary>
    public static float Y {
        get {
            if (!PlayerPrefs.HasKey(YKey))
                PlayerPrefs.SetFloat(YKey, 2f);
            return PlayerPrefs.GetFloat(YKey);
        }
        set {
            PlayerPrefs.SetFloat(YKey, value);
        }
    }
    float startY;

    const string ZKey = "ZPos";
    /// <summary>
    /// Z position of the webcam in cm.
    /// </summary>
    public static float Z {
        get {
            if (!PlayerPrefs.HasKey(ZKey))
                PlayerPrefs.SetFloat(ZKey, 0);
            return PlayerPrefs.GetFloat(ZKey);
        }
        set {
            PlayerPrefs.SetFloat(ZKey, value);
        }
    }
    float startZ;

    /// <summary>
    /// Position of the webcam in cm.
    /// </summary>
    public static Vector3 Position { get { return new Vector3(X, Y, Z); } }

    public delegate void EventHandler();
    public static event EventHandler Changed;

    public enum Axis { X, Y, Z }


    private void Awake()
    {
        // Setut placeholders.
        placeHolderX = xInput.placeholder.GetComponent<Text>();
        placeHolderY = yInput.placeholder.GetComponent<Text>();
        placeHolderZ = zInput.placeholder.GetComponent<Text>();

        // Listen to input fields.
        xInput.onValueChanged.AddListener(delegate (string value) { OnValueChange(value, Axis.X); });
        yInput.onValueChanged.AddListener(delegate (string value) { OnValueChange(value, Axis.Y); });
        zInput.onValueChanged.AddListener(delegate (string value) { OnValueChange(value, Axis.Z); });
    }

    private void OnEnable()
    {
        // Update start values and placeholders.
        startX = X;
        startY = Y;
        startZ = Z;
        placeHolderX.text = startX.ToString();
        placeHolderY.text = startY.ToString();
        placeHolderZ.text = startZ.ToString();

    }


    void OnValueChange(string input, Axis axis)
    {
        if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            StoreValue(value, axis);
        else
            ResetValue(axis);
    }

    void StoreValue(float value, Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                X = value;
                break;
            case Axis.Y:
                Y = value;
                break;
            case Axis.Z:
                Z = value;
                break;
        }
        Changed?.Invoke();
    }

    void ResetValue(Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                StoreValue(startX, axis);
                break;
            case Axis.Y:
                StoreValue(startY, axis);
                break;
            case Axis.Z:
                StoreValue(startZ, axis);
                break;
            default:
                break;
        }
    }
}
