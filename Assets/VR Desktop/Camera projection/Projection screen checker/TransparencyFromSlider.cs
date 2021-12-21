using UnityEngine;

public class TransparencyFromSlider : MonoBehaviour
{
    Renderer rend;
    Color startColor;


    private void Awake()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }


    public void SetTransparencyFromSlider(float value)
    {
        rend.material.color = startColor.SetA(value);
    }
}
