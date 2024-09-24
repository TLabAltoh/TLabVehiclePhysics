using UnityEngine;
using TMPro;
using TLab.UI.SDF;
using TLab.VehiclePhysics;

namespace TLab
{
    public class TachoMeter : MonoBehaviour
    {
        [SerializeField] private Engine m_engine;

        [SerializeField] private SDFArc m_tacho;

        [SerializeField] private TextMeshProUGUI m_gear;

        public void OnShifChanged()
        {
            switch (m_engine.gear)
            {
                case -1:
                    m_gear.text = "R";
                    break;
                case 0:
                    m_gear.text = "N";
                    break;
                default:
                    m_gear.text = m_engine.gear.ToString();
                    break;
            }
        }

        private void Update()
        {
            const float LIMMIT = 0.75f;
            m_tacho.fillAmount = Mathf.Max(m_engine.engineRpm / m_engine.maxEngineRpm, 0) * LIMMIT;
        }
    }
}