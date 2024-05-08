using UnityEngine;

namespace TLab.VehiclePhysics.PacejkaTool
{
    public struct LerpElement
    {
        public int index0;
        public int index1;
        public float factor;
    }

    public struct LerpPacejka
    {
        public Pacejka pacejka0;
        public Pacejka pacejka1;
        public float factor;
    }

    [System.Serializable]
    public class PacejkaDic
    {
        public float index;
        public Color color = Color.red;
        public Pacejka pacejka;
    }

#if UNITY_EDITOR

    [System.Serializable]
    public class GraphSettings
    {
        [SerializeField]
        public float xrange = 20;

        [SerializeField]
        public float yrange = 1;

        [SerializeField]
        public int num = 20;

        [Range(0, 10)]
        public int div = 5;

        [Range(1, 10)]
        public int xAccuracy = 1;

        [Range(1, 10)]
        public int yAccuracy = 1;
    }
#endif
}
