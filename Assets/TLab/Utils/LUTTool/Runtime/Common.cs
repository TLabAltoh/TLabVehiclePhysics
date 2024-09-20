using UnityEngine;

namespace TLab.LUTTool
{
    public struct IndexAndLerpFactor
    {
        public int index0;
        public int index1;
        public float factor;
    }

    public struct LUTAndLerpFactor
    {
        public LUT lut0;
        public LUT lut1;
        public float factor;
    }

#if UNITY_EDITOR

    [System.Serializable]
    public class GraphSettings
    {
        [Min(0)]
        public Vector2Int accuracy = new Vector2Int(1, 1);

        [Min(0)]
        public int param = 5;

        public bool lockX = true;
        public bool lockY = true;
    }
#endif
}
