using UnityEngine;

public static class AniMath
{
    /// <summary>
    /// Smoothen the start and the end of a value going from 0 to 1.
    /// </summary>
    /// <param name="value">Value to smoothen.</param>
    /// <returns>Smoothed value.</returns>
    public static float SmoothStartEnd(float value)
    {
        return 1 - (Mathf.Cos(value * 3.1416f) + 1) * .5f;
    }

    /// <summary>
    /// Smoothen the start of a value going from 0 to 1.
    /// </summary>
    /// <param name="value">Value to smoothen.</param>
    /// <returns>Smoothed value.</returns>
    public static float SmoothStart(float value)
    {
        return 1 - Mathf.Cos(value * 1.5708f);
    }

    /// <summary>
    /// Smoothen the end of a value going from 0 to 1.
    /// </summary>
    /// <param name="value">Value to smoothen.</param>
    /// <returns>Smoothed value.</returns>
    public static float SmoothEnd(float value)
    {
        return Mathf.Sin(value * 1.5708f);
    }
}