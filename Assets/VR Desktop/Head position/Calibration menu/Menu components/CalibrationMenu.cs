using UnityEngine;

/// <summary>
/// Class that navigates calibrationMenu items.
/// </summary>
public class CalibrationMenu : MonoBehaviour {

    int currentItem = 0;
    
    CalibrationMenuItem[] items;

    public delegate void EventHandler();
    public static event EventHandler Calibrated;

    private void Awake()
    {
        items = GetComponentsInChildren<CalibrationMenuItem>(true);
    }

    private void OnEnable()
    {
        OpenFirstMenu();
    }

    public void Next()
    {
        // Go to the next menu or close.
        if (currentItem < items.Length - 1)
        {
            currentItem++;
            OpenItem(currentItem);
        }
        else
            Close();
    }

    public void Previous()
    {
        // Try to open the previous menu.
        if (currentItem > 0)
            return;
        currentItem--;
        OpenItem(currentItem);
    }

    void OpenItem(int index)
    {
        CloseMenus();
        items[index].Open();
    }

    void CloseMenus()
    {
        foreach (CalibrationMenuItem item in items)
            item.Close();
    }

    void OpenFirstMenu()
    {
        currentItem = 0;
        OpenItem(currentItem);
    }

    public void Close()
    {
        Calibrated?.Invoke();
        gameObject.SetActive(false);
    }
}
