using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class ImageColorFromRenderer : MonoBehaviour
{
    [SerializeField]
    Renderer rend;


    private void Start()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (rend)
                GetComponent<Image>().color = rend.material.color;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            if (rend)
                GetComponent<Image>().color = rend.sharedMaterial.color;
#endif
    }
}