using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quits the application if we maintain the Escape key pressed.
/// </summary>
[RequireComponent(typeof(Text))]
public class EchapToQuit : MonoBehaviour
{
    bool quitting;
    float quittingTime;
    [Tooltip("The time during for which we need to keep the escape key pressed.")]
    [SerializeField]
    float duration = 2;
    string quittingPrefix = "Quitting";
    string[] quittingSuffixes = new string[] { "...", ".", "..", "..." };
    Text text;


    private void Reset()
    {
        this.RenameFromType(true);
        this.GetOrAddComponent<Text>().text = quittingPrefix;
    }

    private void Awake()
    {
        text = GetComponent<Text>();
        text.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (!quitting)
            {
                // Start quitting.
                quitting = true;
                quittingTime = Time.time;
                text.enabled = true;
            }
            // Show the quitting suffixes over time, then quit at the end.
            text.text = quittingPrefix + quittingSuffixes[Utilities.IndexFromProgression((Time.time - quittingTime) / duration, quittingSuffixes.Length)];
            if (Time.time - quittingTime > duration)
                Quit();
        }
        else
        {
            if (quitting)
            {
                // Cancel quitting.
                quitting = false;
                text.enabled = false;
            }
        }
    }


    public void Quit()
    {
        // Stop the Editor or close the Application.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}