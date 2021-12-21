using UnityEngine;
using UnityEngine.UI;

public class TextLog : MonoBehaviour
{
    static Text text;


    private void Awake()
    {
        text = GetComponent<Text>();
    }


    public static void Log(object input)
    {
        if (!text)
            return;
        if (text.text.Length > 3000)
            text.text = text.text.Substring(2000);
        text.text += '\n' + input.ToString();
    }
}
