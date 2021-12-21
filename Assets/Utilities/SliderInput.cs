using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(InputField))]
public class SliderInput : MonoBehaviour
{
    [SerializeField]
    Slider slider;
    InputField input;


    private void Awake()
    {
        input = GetComponent<InputField>();
    }

    private void Start()
    {
        UpdateValue(slider.value);
        slider.onValueChanged.AddListener(UpdateValue);
        input.onValueChanged.AddListener(UpdateSlider);
    }


    void UpdateValue(float value)
    {
        input.text = value.ToString();
    }

    void UpdateSlider(string inputText)
    {
        slider.value = float.Parse(inputText);
    }
}