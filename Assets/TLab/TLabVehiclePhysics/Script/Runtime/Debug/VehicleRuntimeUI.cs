using UnityEngine;
using UnityEngine.UIElements;
using TLab.UIElements;

namespace TLab.VehiclePhysics
{
    public class VehicleRuntimeUI : MonoBehaviour
    {
        [SerializeField] private WheelColliderSource m_wheelFR;
        [SerializeField] private WheelColliderSource m_wheelFL;
        [SerializeField] private WheelColliderSource m_wheelRR;
        [SerializeField] private WheelColliderSource m_wheelRL;

        [SerializeField] private UIDocument m_uiDocument;

        private VisualElement m_gageSlipBG;
        private VisualElement m_gageSlipFR;
        private VisualElement m_gageSlipFL;
        private VisualElement m_gageSlipRR;
        private VisualElement m_gageSlipRL;

        void Start()
        {
            VisualElement rootVE = m_uiDocument.rootVisualElement;

            m_gageSlipBG = rootVE.GetElement<VisualElement>("slip", "front", "left", "bg");
            m_gageSlipFL = rootVE.GetElement<VisualElement>("slip", "front", "left", "bg", "fill");
            m_gageSlipFR = rootVE.GetElement<VisualElement>("slip", "front", "right", "bg", "fill");
            m_gageSlipRL = rootVE.GetElement<VisualElement>("slip", "rear", "left", "bg", "fill");
            m_gageSlipRR = rootVE.GetElement<VisualElement>("slip", "rear", "right", "bg", "fill");
        }

        void Update()
        {
            float height = m_gageSlipBG.resolvedStyle.height;
            m_gageSlipFL.style.height = Mathf.Abs(m_wheelFL.feedbackSlipRatio) * height;
            m_gageSlipFR.style.height = Mathf.Abs(m_wheelFR.feedbackSlipRatio) * height;
            m_gageSlipRL.style.height = Mathf.Abs(m_wheelRL.feedbackSlipRatio) * height;
            m_gageSlipRR.style.height = Mathf.Abs(m_wheelRR.feedbackSlipRatio) * height;
        }
    }
}
