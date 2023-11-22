using UnityEngine;

namespace TLab.VehiclePhysics
{
    [CreateAssetMenu()]
    public class VehicleEngineInfo : ScriptableObject
    {
        public Gear[] Gears { get => m_gears; }

        public LUT torqueCurve { get => m_torqueCurve; }

        [SerializeField] private Gear[] m_gears;
        [SerializeField] private LUT m_torqueCurve;
    }

    [System.Serializable]
    public class Gear
    {
        public int gear;
        public float ratio;
    }
}
