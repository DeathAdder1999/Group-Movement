using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;

public static class Utils
{
    private static Random _random = new Random();

    public static float Distance(Vector2 v1, Vector2 v2)
    {
        return Mathf.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y));
    }

    //If distance between v1 and v2 is less than tolerance
    public static bool IsCloserThan(Vector2 v1, Vector2 v2, float tolerance)
    {
        return (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) < tolerance * tolerance;
    }

    public static Vector2 Truncate(Vector2 v, float magnitude)
    {
        if (v.sqrMagnitude > magnitude * magnitude)
        {
            v.Normalize();
            v *= magnitude;
        }

        return v;
    }

    public static Vector2 Clamp(Vector2 v, float minValue, float maxValue)
    {
        if (v.sqrMagnitude > maxValue * maxValue)
        {
            v.Normalize();
            v *= maxValue;
            return v;
        }

        if (v.sqrMagnitude < minValue * minValue)
        {
            v.Normalize();
            v *= minValue;
            return v;
        }

        return v;
    }

    public static float GetRandomFloat(float min, float max)
    {
        return (float) _random.NextDouble() * (max - min) + min;
    }

    public static Vector2 GetRandomVector(Vector2 xLimits, Vector2 yLimits)
    {
        return new Vector2(GetRandomFloat(xLimits.x, xLimits.y), GetRandomFloat(yLimits.x, yLimits.y));
    }

    public static int GetMaximumDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Math.Abs(pos1.x - pos2.x) + Math.Abs(pos1.y - pos2.y);
    }

    public static int GetMinimumDistance(Vector2Int pos1, Vector2Int pos2)
    {
        var xDistance = Math.Abs(pos1.x - pos2.x);
        var yDistance = Math.Abs(pos1.y - pos2.y);

        return xDistance + (xDistance - yDistance);
    }
}
