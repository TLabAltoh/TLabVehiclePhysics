using UnityEngine;
using TMPro;
using TLab.UI;
using TLab.VehiclePhysics;

namespace TLab
{
    public class TachoMeter : MonoBehaviour
    {
        [SerializeField] private VehicleEngine m_engine;

        [SerializeField] private SDFRing m_tacho;

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
            const float limmit = 0.8f;

            const float offset = 0.7f;

            var theta = m_engine.engineRpm / m_engine.maxEngineRpm * (Mathf.PI * limmit);

            m_tacho.material.SetFloat(SDFUI.PROP_THETA, theta);

            m_tacho.transform.rotation =
                Quaternion.Euler(
                    new Vector3(0f, 0f, -(theta - (Mathf.PI * offset)) * Mathf.Rad2Deg));
        }
    }
}