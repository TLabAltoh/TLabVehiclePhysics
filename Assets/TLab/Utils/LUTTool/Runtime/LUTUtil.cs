using UnityEngine;

namespace TLab.LUTTool
{
    public struct LerpElement
    {
        public int index0;
        public int index1;
        public float factor;
    }

    public struct LerpLUT
    {
        public LUT lut0;
        public LUT lut1;
        public float factor;
    }

    [System.Serializable]
    public class LUTDic
    {
        public float index;
        public Color color = Color.red;
        public LUT lut;
    }

#if UNITY_EDITOR

    [System.Serializable]
    public class GraphSettings
    {
        [Range(0, 10)]
        public int div = 5;

        [Range(1, 10)]
        public int xAccuracy = 1;

        [Range(1, 10)]
        public int yAccuracy = 1;
    }
#endif
}
