using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TLabLUT : ScriptableObject
{
    public float[] indexs;
    public float[] values;

    public float Evaluate(float x)
    {
        if (x <= indexs[0])
        {
            return values[0];
        }
        else if (x >= indexs[indexs.Length - 1])
        {
            return values[indexs.Length - 1];
        }
        else
        {
            return TLabLUTCalc.BinaryLerp(0, indexs.Length - 1, indexs, values, x);
        }
    }
}
