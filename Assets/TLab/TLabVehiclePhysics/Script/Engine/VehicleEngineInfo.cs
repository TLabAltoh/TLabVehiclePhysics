using UnityEngine;

namespace TLab.VehiclePhysics
{
    [CreateAssetMenu(menuName = "TLab/VehiclePhysics/EngineInfo", fileName = "EngineInfo")]
    public class VehicleEngineInfo : ScriptableObject
    {
        public GearInfo[] gearInfos { get => m_gearInfos; }

        public MultiLUT torqueCurve { get => m_torqueCurve; }

        [SerializeField] private GearInfo[] m_gearInfos;
        [SerializeField] private MultiLUT m_torqueCurve;
    }

    [System.Serializable]
    public class GearInfo
    {
        public int gear;
        public float ratio;
    }
}
