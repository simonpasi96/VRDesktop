using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hides children texts and images when the cursor is hidden.
/// </summary>
public class CursorVisibilityFollower : MonoBehaviour
{
    Dictionary<Text, float> defaultTextsAlphas = new Dictionary<Text, float>();
    Dictionary<Image, float> defaultImagesAlphas = new Dictionary<Image, float>();
    bool isHiding = false;


    private void Start()
    {
        // Get all texts and images default alphas.
        foreach (Text text in GetComponentsInChildren<Text>())
            defaultTextsAlphas.Add(text, text.color.a);
        foreach (Image image in GetComponentsInChildren<Image>())
            defaultImagesAlphas.Add(image, image.color.a);

    }

    private void Update()
    {
        if (!Cursor.visible && !isHiding)
        {
            // Hide.
            StopAllCoroutines();
            this.ProgressionAnim(.3f, delegate (float progression)
            {
                foreach (KeyValuePair<Text, float> textPair in defaultTextsAlphas)
                    textPair.Key.color = textPair.Key.color.SetA(1 - progression);
                foreach (KeyValuePair<Image, float> imagePair in defaultImagesAlphas)
                    imagePair.Key.color = imagePair.Key.color.SetA(1 - progression);
            });
            isHiding = true;
        }
        else if (Cursor.visible && isHiding)
        {
            // Unhide.
            StopAllCoroutines();
            foreach (KeyValuePair<Text, float> textPair in defaultTextsAlphas)
                textPair.Key.color = textPair.Key.color.SetA(textPair.Value);
            foreach (KeyValuePair<Image, float> imagePair in defaultImagesAlphas)
                imagePair.Key.color = imagePair.Key.color.SetA(imagePair.Value);
            isHiding = false;
        }

    }
}