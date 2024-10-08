using UnityEngine;

namespace TLab.VehiclePhysics.PacejkaTool
{
    public struct IndexAndLerpFactor
    {
        public int index0;
        public int index1;
        public float factor;
    }

    public struct PacejkaAndLerpFactor
    {
        public Pacejka pacejka0;
        public Pacejka pacejka1;
        public float factor;
    }

#if UNITY_EDITOR

    [System.Serializable]
    public class GraphSettings
    {
        public Vector2Int range = new Vector2Int(1, 1);

        [Min(0)]
        public Vector2Int param = new Vector2Int(20, 5);

        [Min(0)]
        public Vector2Int accuracy = new Vector2Int(1, 1);
    }
#endif
}
