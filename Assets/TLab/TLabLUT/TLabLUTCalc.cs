using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TLabLUTCalc
{
    public static float BinaryLerp(int L, int H, float[] s, float[] t, float x)
    {
        int M = (L + H) / 2;
        bool a = x >= s[M];
        bool b = x <= s[M + 1];

        if (L <= H && a && b)
        {
            float lerpFactor = (x - s[M]) / (s[M + 1] - s[M]);
            return t[M] * (1 - lerpFactor) + t[M + 1] * lerpFactor;
        }
        else if (!a && b)
        {
            return BinaryLerp(L, M, s, t, x);
        }
        else
        {
            return BinaryLerp(M, H, s, t, x);
        }
    }
}
