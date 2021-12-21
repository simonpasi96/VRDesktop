#if UNITY_EDITOR
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldFloat : MonoBehaviour
{
    [SerializeField]
    FloatEvent OnValueChanged, OnEndEdit;


    private void Reset()
    {
        InputField field = GetComponent<InputField>();
        field.contentType = InputField.ContentType.DecimalNumber;
        UnityEventTools.AddPersistentListener(field.onValueChanged, OnValChangedCaller);
        UnityEventTools.AddPersistentListener(field.onEndEdit, OnEndEditCaller);
    }

    void OnValChangedCaller(string input)
    {
        TextLog.Log("start");
        float value;
        if (float.TryParse(input, out value))
            OnValueChanged?.Invoke(value);
    }
    void OnEndEditCaller(string input)
    {
        TextLog.Log("end");
        float value;
        if (float.TryParse(input, out value))
            OnEndEdit?.Invoke(value);
    }
}

[System.Serializable]
class FloatEvent : UnityEvent<float>
{

}
#endif