using UnityEngine;
using UnityEngine.UIElements;
using TLab.UIElements;

namespace TLab.VehiclePhysics
{
    public class VehicleDebugUI : MonoBehaviour
    {
        [SerializeField] private WheelColliderSource m_wheelFR;
        [SerializeField] private WheelColliderSource m_wheelFL;
        [SerializeField] private WheelColliderSource m_wheelRR;
        [SerializeField] private WheelColliderSource m_wheelRL;

        [SerializeField] private VehiclePhysics m_physics;
        [SerializeField] private VehicleEngine m_engine;

        [SerializeField] private UIDocument m_uiDocument;

        private Label m_labelRpm;
        private Label m_labelGear;
        private Label m_labelAngle;
        private Label m_labelSpeed;
        private Label m_labelGripR;
        private Label m_labelGripF;

        void Start()
        {
            VisualElement rootVE = m_uiDocument.rootVisualElement;
            Button button = rootVE.Q<Button>("reset");
            button.clicked += () =>
            {
                m_physics.rb.transform.up = Vector3.up;
            };

            m_labelRpm = rootVE.GetElement<Label>("rpm", "value");
            m_labelGear = rootVE.GetElement<Label>("gear", "value");
            m_labelAngle = rootVE.GetElement<Label>("angle", "value");
            m_labelSpeed = rootVE.GetElement<Label>("speed", "value");
            m_labelGripR = rootVE.GetElement<Label>("grip_r", "value");
            m_labelGripF = rootVE.GetElement<Label>("grip_f", "value");
        }

        void Update()
        {
            m_labelRpm.text = m_engine.engineRpm.ToString();
            m_labelGear.text = m_engine.gear.ToString();
            m_labelAngle.text = m_physics.minAngle.ToString("0.00");
            m_labelSpeed.text = m_physics.kilometerPerHourInLocal.ToString("0.0");
            m_labelGripR.text = m_physics.downforceRear.ToString("0.00");
            m_labelGripF.text = m_physics.downforceFront.ToString("0.00");
        }
    }
}
