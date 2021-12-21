using OpenCVForUnity.CoreModule;
using UnityEngine;

public static class OpenCVForUnityUtilities
{
    public static Point ToPoint(this Vector2 vector)
    {
        return new Point(vector.x, vector.y);
    }
    public static Vector2 ToVector2(this Point point)
    {
        return new Vector2((float)point.x, (float)point.y);
    }

    public static Scalar ToScalar(this Color color)
    {
        return new Scalar(color.r, color.g, color.b, color.a)*255;
    }
}
