using UnityEngine;

namespace TLab.LUTUtil
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
}
