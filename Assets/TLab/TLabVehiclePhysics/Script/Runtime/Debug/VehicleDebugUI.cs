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

        [SerializeField] private AeroDynamics m_areoDynamics;
        [SerializeField] private Engine m_engine;

        [SerializeField] private UIDocument m_uiDocument;

        private Label m_labelRpm;
        private Label m_labelGear;
        private Label m_labelAngle;
        private Label m_labelSpeed;
        private Label m_labelGripR;
        private Label m_labelGripF;
        private Label m_labelTorqueR;
        private Label m_labelTorqueL;
        private Label m_labelRawWheelRpmL;
        private Label m_labelRawWheelRpmR;
        private Label m_labelWheelRpmL;
        private Label m_labelWheelRpmR;
        private Label m_labelWheelForceL;
        private Label m_labelWheelForceR;

        void Start()
        {
            VisualElement rootVE = m_uiDocument.rootVisualElement;
            Button button = rootVE.Q<Button>("reset");
            button.clicked += () =>
            {
                m_areoDynamics.rb.transform.up = Vector3.up;
            };

            m_labelRpm = rootVE.GetElement<Label>("rpm", "value");
            m_labelGear = rootVE.GetElement<Label>("gear", "value");
            m_labelAngle = rootVE.GetElement<Label>("angle", "value");
            m_labelSpeed = rootVE.GetElement<Label>("speed", "value");

            m_labelGripR = rootVE.GetElement<Label>("grip_r", "value");
            m_labelGripF = rootVE.GetElement<Label>("grip_f", "value");

            m_labelTorqueR = rootVE.GetElement<Label>("torque_r", "value");
            m_labelTorqueL = rootVE.GetElement<Label>("torque_l", "value");

            m_labelRawWheelRpmR = rootVE.GetElement<Label>("raw_wheel_rpm_r", "value");
            m_labelRawWheelRpmL = rootVE.GetElement<Label>("raw_wheel_rpm_l", "value");

            m_labelWheelRpmR = rootVE.GetElement<Label>("wheel_rpm_r", "value");
            m_labelWheelRpmL = rootVE.GetElement<Label>("wheel_rpm_l", "value");

            m_labelWheelForceL = rootVE.GetElement<Label>("wheel_force_l", "value");
            m_labelWheelForceR = rootVE.GetElement<Label>("wheel_force_r", "value");
        }

        void Update()
        {
            m_labelRpm.text = m_engine.engineRpm.ToString();
            m_labelGear.text = m_engine.gear.ToString();
            m_labelAngle.text = m_areoDynamics.minAngle.ToString("0.00");
            m_labelSpeed.text = m_areoDynamics.kilometerPerHourInLocal.ToString("0.0");

            m_labelGripR.text = m_areoDynamics.downforceRear.ToString("0.00");
            m_labelGripF.text = m_areoDynamics.downforceFront.ToString("0.00");

            m_labelTorqueR.text = m_wheelRR.finalTorque.ToString("0.00");
            m_labelTorqueL.text = m_wheelRL.finalTorque.ToString("0.00");

            m_labelRawWheelRpmR.text = m_wheelRR.rawWheelRpm.ToString("0.00");
            m_labelRawWheelRpmL.text = m_wheelRL.rawWheelRpm.ToString("0.00");

            m_labelWheelRpmR.text = m_wheelRR.wheelRpm.ToString("0.00");
            m_labelWheelRpmL.text = m_wheelRL.wheelRpm.ToString("0.00");

            m_labelWheelForceL.text = m_wheelRL.totalTireForceLocal.ToString("+#;-#;0.00;");
            m_labelWheelForceR.text = m_wheelRR.totalTireForceLocal.ToString("+#;-#;0.00;");
        }
    }
}
