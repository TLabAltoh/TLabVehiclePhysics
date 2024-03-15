using UnityEngine;

namespace TLab.VehiclePhysics
{
    [CreateAssetMenu(menuName = "TLab/VehiclePhysics/EngineInfo", fileName = "EngineInfo")]
    public class VehicleEngineInfo : ScriptableObject
    {
        public enum Transmission
        {
            AT,
            MT
        }

        [System.Serializable]
        public class GearInfo
        {
            public int gear;
            public float ratio;

            public int maxRpmThreshold;
            public int minRpmThreshold;
        }

        public GearInfo[] gearInfos => m_gearInfos;

        public Transmission transmission => m_transmission;

        public MultiLUT torqueCurve => m_torqueCurve;

        public float shiftChangeIntervals => m_shiftChangeIntervals;

        [Header("Gear Index/Ratio")]
        [Tooltip("gear > 0 -> drive, gear < 0 -> reverse, gear == 0 -> neutral")]
        [SerializeField] private GearInfo[] m_gearInfos;

        [Header("Transmission Mode (AT/MT)")]
        [SerializeField] private Transmission m_transmission;

        [Header("Engine's Torque Curve")]
        [SerializeField] private MultiLUT m_torqueCurve;

        [Space(10)]
        [SerializeField] private float m_shiftChangeIntervals;
    }
}
