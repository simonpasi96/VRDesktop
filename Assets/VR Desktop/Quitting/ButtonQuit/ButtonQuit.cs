using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonQuit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField]
    Image crossImage;
    Color crossStartColor;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        crossStartColor = crossImage.color;
    }

    void OnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        crossImage.color = Color.white;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        crossImage.color = crossStartColor;
    }
}
