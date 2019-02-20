using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}

public static class TransformExtension
{
    public static Bounds TransformBounds(this Transform transform, Bounds localBounds)
    {
        var center = transform.TransformPoint(localBounds.center);

        var extents = localBounds.extents;
        var axisX = transform.TransformVector(extents.x, 0, 0);
        var axisY = transform.TransformVector(0, extents.y, 0);
        var axisZ = transform.TransformVector(0, 0, extents.z);

        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }

    public static Bounds InverseTransformBounds(this Transform transform, Bounds worldBounds)
    {
        var center = transform.InverseTransformPoint(worldBounds.center);

        var extents = worldBounds.extents;
        var axisX = transform.InverseTransformVector(extents.x, 0, 0);
        var axisY = transform.InverseTransformVector(0, extents.y, 0);
        var axisZ = transform.InverseTransformVector(0, 0, extents.z);
        
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }
}

public static class PlatformEffector2DExtension
{
    public static bool ValidCollision(this PlatformEffector2D effector, Vector2 velocity)
    {
        float dot = Vector2.Dot(effector.transform.up, -velocity.normalized);
        float cos = Mathf.Cos(effector.surfaceArc * 0.5f * Mathf.Deg2Rad);

        dot = Mathf.Round(dot * 1000.0f) / 1000.0f;
        cos = Mathf.Round(cos * 1000.0f) / 1000.0f;

        if (dot > cos)
        {
            return true;
        }

        return false;
    }
}
