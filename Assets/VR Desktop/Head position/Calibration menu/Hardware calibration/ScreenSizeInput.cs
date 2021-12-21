using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSizeInput : MonoBehaviour {

    [SerializeField]
    InputField diagonalField, aspectRatioFieldW, aspectRatioFieldH;
    Text diagonalFieldPlaceholder, aspectRatioFieldWPlaceholder, aspectRatioFieldHPlaceholder;

    const string AspectRatioKey = "AspectRatio";
    public static AspectRatio AspectRatio {
        get {
            if (!PlayerPrefs.HasKey(AspectRatioKey))
                PlayerPrefs.SetString(AspectRatioKey, 16.ToString() + ' ' + 9.ToString());
            string[] valueInStrings = PlayerPrefs.GetString(AspectRatioKey).Split(' ');
            return new AspectRatio(int.Parse(valueInStrings[0]), int.Parse(valueInStrings[1]));
        }
        set {
            PlayerPrefs.SetString(AspectRatioKey, value.Width.ToString() + ' ' + value.Height.ToString());
        }
    }
    AspectRatio startAspectRatio;

    const string DiagonalKey = "Diagonal";
    public static float Diagonal {
        get {
            if (!PlayerPrefs.HasKey(DiagonalKey))
                PlayerPrefs.SetFloat(DiagonalKey, 58);
            return PlayerPrefs.GetFloat(DiagonalKey);
        }
        set {
            PlayerPrefs.SetFloat(DiagonalKey, value);
        }
    }
    float startDiagonal;

    /// <summary>
    /// Dimensions of the screen in cm.
    /// </summary>
    public static Vector2 WidthHeight { get { return WidthHeightFromDiagonalAndAspectRatio(Diagonal, AspectRatio); } }

    public delegate void EventHandler();
    public static event EventHandler DiagonalChanged;


    private void Awake()
    {
        // Setup placeholders.
        diagonalFieldPlaceholder = diagonalField.placeholder.GetComponent<Text>();
        aspectRatioFieldWPlaceholder = aspectRatioFieldW.placeholder.GetComponent<Text>();
        aspectRatioFieldHPlaceholder = aspectRatioFieldH.placeholder.GetComponent<Text>();

        // Listen to input fields.
        diagonalField.onValueChanged.AddListener(OnDiagonalInput);
        aspectRatioFieldW.onValueChanged.AddListener(OnAspectRatioWidthInput);
        aspectRatioFieldH.onValueChanged.AddListener(OnAspectRatioHeightInput);
    }

    private void OnEnable()
    {
        // Update start values and placeholders.
        startDiagonal = Diagonal;
        startAspectRatio = AspectRatio;
        diagonalFieldPlaceholder.text = startDiagonal.ToString("0.00");
        aspectRatioFieldWPlaceholder.text = startAspectRatio.Width.ToString();
        aspectRatioFieldHPlaceholder.text = startAspectRatio.Height.ToString();
    }


    #region OnInput -------
    void OnDiagonalInput(string input)
    {
        // If we can't parse, it means that the field's empty. So we update the placeholder.
        if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            Diagonal = value;
        else
            Diagonal = startDiagonal;
        DiagonalChanged?.Invoke();
    }

    void OnAspectRatioWidthInput(string input)
    {
        // If we can't parse, it means that the field's empty. So we update the placeholder.
        if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int value))
            AspectRatio.SetWidth(value);
        else
            AspectRatio.SetWidth(startAspectRatio.Width);
    }

    void OnAspectRatioHeightInput(string input)
    {
        // If we can't parse, it means that the field's empty. So we update the placeholder.
        if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int value))
            AspectRatio.SetHeight(value);
        else
            AspectRatio.SetHeight(value);
    }
    #endregion --------------


    static Vector2 WidthHeightFromDiagonalAndAspectRatio(float diagonal, AspectRatio aspectRatio)
    {
        float width = diagonal * (aspectRatio.Width / Mathf.Sqrt(Mathf.Pow(aspectRatio.Width, 2) + Mathf.Pow(aspectRatio.Height, 2)));
        float height = diagonal * (aspectRatio.Height / Mathf.Sqrt(Mathf.Pow(aspectRatio.Width, 2) + Mathf.Pow(aspectRatio.Height, 2)));
        return new Vector2(width, height);
    }
}


public class AspectRatio {
    public int Width { get; private set; }
    public int Height { get; private set; }
    public AspectRatio(int width, int height)
    {
        Width = width;
        Height = height;
    }
    public void SetWidth(int newWidth)
    {
        Width = newWidth;
    }
    public void SetHeight(int newHeight)
    {
        Height = newHeight;
    }
}