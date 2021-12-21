using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class Utilities
{
    #region ProgressionAnim -------------------------------
    public static IEnumerator ProgressionAnim(this MonoBehaviour caller, float duration, Action<float> progressionHandler, Action endActionHandler = null)
    {
        if (!caller.gameObject.activeInHierarchy)
            return null;
        IEnumerator routine = ProgressionAnim(duration, progressionHandler, endActionHandler);
        caller.StartCoroutine(routine);
        return routine;
    }

    static IEnumerator ProgressionAnim(float duration, Action<float> progressionHandler, Action endActionHandler)
    {
        float progression = 0;
        while (progression <= 1)
        {
            progressionHandler(progression);
            progression += Time.deltaTime / duration;
            yield return null;
        }
        progressionHandler(1);
        endActionHandler?.Invoke();
    }
    #endregion --------------------------------------------

    #region Timer --------
    public static IEnumerator Timer(this MonoBehaviour caller, float duration, System.Action endActionHandler)
    {
        IEnumerator routine = TimerRoutine(duration, endActionHandler);
        caller.StartCoroutine(routine);
        return routine;
    }

    static IEnumerator TimerRoutine(float duration, System.Action endActionHandler)
    {
        yield return new WaitForSeconds(duration);
        endActionHandler();
    }
    #endregion ------------

    #region Show/hide/destroy gameObject children ------------------
    /// <summary>
    /// Set all children active.
    /// </summary>
    /// <param name="go"></param>
    public static void ShowChildren(this GameObject go)
    {
        go.transform.SetChildrenActive(true);
    }
    /// <summary>
    /// Set all children inactive.
    /// </summary>
    /// <param name="go"></param>
    public static void HideChildren(this GameObject go)
    {
        go.transform.SetChildrenActive(false);
    }

    static void SetChildrenActive(this Transform transform, bool state)
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(state);
    }

    public static void DestroyChidren(this Transform transform)
    {
        if (transform == null || transform.childCount < 1)
            return;
        for (int i = transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
    }
    #endregion ------------------------------------

    #region Add or duplicate components --------------------
    public static T GetOrAddComponent<T>(this Component component) where T : Component
    {
        if (component.GetComponent<T>() != null)
            return component.GetComponent<T>();
        else
            return component.gameObject.AddComponent<T>();
    }

    #region Duplicate a component ---------------------------
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType())
            return null;
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { }
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
    #endregion ------------------------------------------------------
    #endregion --------------------------------------------

    /// <summary>
    /// Returns a random integer number between min [inclusive] and max [exclusive]. Never returns something equal to excluded.
    /// </summary>
    public static int ExclusiveRange(int min, int max, int excluded)
    {
        if (excluded < min || excluded >= max)
            return UnityEngine.Random.Range(min, max);
        int result = UnityEngine.Random.Range(min, max - 1);
        if (result >= excluded)
            result++;
        return result;
    }

    public static void LimitedIncrement(ref int index, int arrayLength)
    {
        index++;
        if (index >= arrayLength)
            index = 0;
    }
    public static void LimitedDecrement(ref int index, int arrayLength)
    {
        index--;
        if (index < 0)
            index = arrayLength - 1;
    }

    public static bool IsBetween(this float number, float a, float b)
    {
        return (number >= a && number <= b) || (number <= a && number >= b);
    }

    public static void Reset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    public static float RoundedDecimals(this float number, int decimalPlaces)
    {
        return (float)Math.Round(number, decimalPlaces);
    }

    #region Set Vector2 values ---------------------
    /// <summary>
    /// Changes X in the Vector and returns the Vector.
    /// </summary>
    public static Vector2 SetX(this Vector2 vector2, float value)
    {
        return new Vector2(value, vector2.y);
    }
    /// <summary>
    /// Changes Y in the Vector and returns the Vector.
    /// </summary>
    public static Vector2 SetY(this Vector2 vector2, float value)
    {
        return new Vector2(vector2.x, value);
    }
    #endregion ------------------------------------------
    #region Set Vector3 values ---------------------
    /// <summary>
    /// Changes X in the Vector and returns the Vector.
    /// </summary>
    public static Vector3 SetX(this Vector3 vector3, float value)
    {
        return new Vector3(value, vector3.y, vector3.z);
    }
    /// <summary>
    /// Changes Y in the Vector and returns the Vector.
    /// </summary>
    public static Vector3 SetY(this Vector3 vector3, float value)
    {
        return new Vector3(vector3.x, value, vector3.z);
    }
    /// <summary>
    /// Changes Z in the Vector and returns the Vector.
    /// </summary>
    public static Vector3 SetZ(this Vector3 vector3, float value)
    {
        return new Vector3(vector3.x, vector3.y, value);
    }
    #endregion ------------------------------------------
    #region Set Color values ---------------------
    /// <summary>
    /// Changes R in the Color and returns the Color.
    /// </summary>
    public static Color SetR(this Color color, float value)
    {
        return new Color(value, color.g, color.b, color.a);
    }
    /// <summary>
    /// Changes G in the Color and returns the Color.
    /// </summary>
    public static Color SetG(this Color color, float value)
    {
        return new Color(color.r, value, color.b, color.a);
    }
    /// <summary>
    /// Changes B in the Color and returns the Color.
    /// </summary>
    public static Color SetB(this Color color, float value)
    {
        return new Color(color.r, color.g, value, color.a);
    }
    /// <summary>
    /// Changes A in the Color and returns the Color.
    /// </summary>
    public static Color SetA(this Color color, float value)
    {
        return new Color(color.r, color.g, color.b, value);
    }
    #endregion ------------------------------------------

    #region Remapping ----------
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    /// <summary>
    /// Maps a value going from -1 to 1 to a value going from 0 to 1.
    /// </summary>
    /// <param name="value">Value to remap.</param>
    /// <returns></returns>
    public static float To01(this float value)
    {
        return (value + 1) * .5f;
    }
    /// <summary>
    /// Maps a Vector2 going from -1 to 1 to a Vector2 going from 0 to 1.
    /// </summary>
    /// <param name="value">Vector2 to remap.</param>
    /// <returns></returns>
    public static Vector2 To01(this Vector2 value)
    {
        return (value + Vector2.one) * .5f;
    }
    /// <summary>
    /// Maps a value going from 0 to 1 to a value going from -1 to 1.
    /// </summary>
    /// <param name="value">Value to remap.</param>
    /// <returns></returns>
    public static float ToMin1_1(this float value)
    {
        return value * 2 - 1;
    }
    /// <summary>
    /// Maps a value going from 0 to 1 to a value going from -1 to 1.
    /// </summary>
    /// <param name="value">Value to remap.</param>
    /// <returns></returns>
    public static Vector2 ToMin1_1(this Vector2 value)
    {
        return (value * 2) - Vector2.one;
    }

    public static int IndexFromProgression(float progression, int slices)
    {
        return (int)Mathf.Clamp(progression / (1 / (float)slices), 0, slices - 1);
    }
    #endregion --------------------

    /// <summary>
    /// Returns true if the routine is null.
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    public static bool IsRunning(this IEnumerator routine)
    {
        return routine != null;
    }

    /// <summary>
    /// Add an action to the UnityEvent displayed in the editor.
    /// </summary>
    /// <param name="button">The object that will listen to the event.</param>
    /// <param name="action">The action that will be triggered after the event.</param>
    /// <returns>Whether the action was successfully added.</returns>
    public static bool AddPersistentEvent(this Button button, UnityEngine.Object caller, UnityAction action)
    {
#if UNITY_EDITOR
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            if (button.onClick.GetPersistentTarget(i) == caller && button.onClick.GetPersistentMethodName(i) == action.Method.Name)
                return false;
        UnityEventTools.AddPersistentListener(button.onClick, action);
#endif
        return false;
    }

    public static Vector2 Vector2Center(Vector2 A, Vector2 B)
    {
        return ((B - A) * .5f) + A;
    }

    public static Vector2 PointOnCircle(float radius, float angle)
    {
        Vector2 pos;
        pos.x = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        return pos;
    }

    public static float ListAverage(List<float> listOfPositions)
    {
        // Return the average magnitude between each point of the list of positions.
        if (listOfPositions.Count == 0)
            return 0;
        float sum = 0;
        for (int i = 0; i < listOfPositions.Count - 1; i++)
            sum = listOfPositions[i + 1] - listOfPositions[i];
        return sum / listOfPositions.Count;
    }

    /// <summary>
    /// Set the name of this component's gameObject from the name of this component.
    /// </summary>
    /// <param name="mono"></param>
    public static void RenameFromType(this MonoBehaviour mono, bool onlyIfDefaultName = false)
    {
        if (onlyIfDefaultName && mono.gameObject.name != "GameObject")
            return;
        string newName = mono.GetType().ToString();
        if (newName.Length > 1)
            for (int i = 1; i < newName.Length; i++)
                if (char.IsUpper(newName[i]))
                {
                    newName = newName.Insert(i, " ");
                    i++;
                }
        mono.gameObject.name = newName;
    }

    /// <summary>
    /// Clamp the position of the rect to the area. (only works on XY)
    /// </summary>
    /// <param name="rect">The rect to clamp.</param>
    /// <param name="area">The area in which the rect must stay.</param>
    public static void ClampToRect(this RectTransform rect, RectTransform area)
    {
        Vector3[] corners = new Vector3[4];
        area.GetWorldCorners(corners);
        Vector2 newPosition = new Vector2(Mathf.Clamp(rect.position.x, corners[0].x, corners[2].x), Mathf.Clamp(rect.position.y, corners[0].y, corners[2].y));
        rect.position = newPosition;
    }
    
    #region Indent ----------
    /// <summary>
    /// Add an amount of tabulations to the beginning of the string.
    /// </summary>
    /// <param name="s">String to indent. If negative, will unindent.</param>
    /// <param name="amount">Amount of tabulations to add.</param>
    /// <returns></returns>
    public static string Indent(this string s, int amount = 1)
    {
        if (amount < 0)
            return s.Unindent(-amount);
        string indents = "";
        for (int i = 0; i < amount; i++)
            indents += "\t";
        return indents + s.Replace("\n", "\n" + indents);
    }

    /// <summary>
    /// Removes an amount of tabulations (or all) to the beginning of the string.
    /// </summary>
    /// <param name="s">String to unindent. If positive, will indent.</param>
    /// <param name="amount">Amount of tabulations to remove.</param>
    /// <param name="full">Remove all tabulations ?</param>
    /// <returns></returns>
    public static string Unindent(this string s, int amount = 1)
    {
        if (amount == 0)
            return s;
        else if (amount < 0)
            return s.Indent(-amount);
        for (int i = 0; i < amount; i++)
            s = s.Replace("\n\t", "\n");
        return s;
    }

    public static string UnindentFull(this string s)
    {
        // Remove all tabs and spaces after a carriage return.
        return Regex.Replace(s, @"\n(\t| )+", "\n");
    }
    #endregion -----------------
}