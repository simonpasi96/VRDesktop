using UnityEngine;
using UnityEngine.UI;

public class CalibrationMenuItem : MonoBehaviour {
    [SerializeField]
    protected Button ContinueButton;

    protected virtual void Awake()
    {
        ContinueButton.onClick.AddListener(Continue);
    }

    void Continue()
    {
        if (!CanContinue())
            return;
        OnContinue();
        GetComponentInParent<CalibrationMenu>().Next();
    }

    protected virtual bool CanContinue() { return true; }
    protected virtual void OnContinue() { }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
