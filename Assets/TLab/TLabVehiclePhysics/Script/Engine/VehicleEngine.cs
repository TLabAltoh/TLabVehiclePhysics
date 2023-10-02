using UnityEngine;

namespace TLab.VehiclePhysics
{
    public class VehicleEngine : MonoBehaviour
    {
        public enum State
        {
            On,
            Off
        }

        [SerializeField] private WheelColliderSource[] m_driveWheels;
        [SerializeField] private WheelColliderSource[] m_brakeWheels;

        [Space(10)]
        [SerializeField] private VehicleEngineInfo m_engineInfo;

        [Space(10)]
        [SerializeField] private VehicleSystemManager m_systemManager;
        [SerializeField] private VehiclePhysics m_vihiclePhysics;
        [SerializeField] private VehicleInputManager m_inputManager;

        private int m_currentGearIndex = 2;
        private int m_currentGear;
        private float m_currentGearRatio;

        private State m_currentState = State.Off;

        private float m_maxRpm;
        private float m_engineRpm = IDLING;

        private const float FIXED_TIME = 30f;
        private const float IDLING = 1400f;
        private const float RPM_INCREMENT = 250f;
        private const float RPM_ATTENUATION = 50f;

        public int CurrentGear => m_currentGear;

        public float EngineRpm => m_engineRpm;

        public float EngineMaxRpm => m_maxRpm;

        private float TimeError => Time.fixedDeltaTime * FIXED_TIME;

        private float ActualInput => m_inputManager.ActualInput;

        private float ClutchInput => m_inputManager.ClutchInput;

        public float Pitch => 1f + ((m_engineRpm - IDLING) / (m_maxRpm - IDLING)) * 2f;

        private bool TransmissionDisconnected => m_currentGearRatio == 0 || ClutchInput > 0.5f || (m_engineRpm < IDLING && ActualInput < 0.1f);

        private bool GearUpPressed
        {
            get => m_inputManager.GearUpPressed;
            set => m_inputManager.GearUpPressed = value;
        }

        private bool GearDownPressed
        {
            get => m_inputManager.GearDownPressed;
            set => m_inputManager.GearDownPressed = value;
        }

        public State CurrentState => m_currentState;

        private float GetFeedbackRPM()
        {
            float rpmSum = 0f;
            foreach (WheelColliderSource wheelOutput in m_driveWheels)
            {
                rpmSum += wheelOutput.FeedbackRpm;
            }

            float feedbackRPM = rpmSum / m_driveWheels.Length;

            return feedbackRPM;
        }

        private float GetTorque()
        {
            return m_engineInfo.TorqueCurve.Evaluate(m_engineRpm) * ActualInput;
        }

        private float GetMaxRPM()
        {
            float[] indexs = m_engineInfo.TorqueCurve.indexs;
            return indexs[indexs.Length - 1];
        }

        private float GetAccelerated(float feedbackRPM)
        {
            return TLab.Math.LinerApproach(feedbackRPM, RPM_INCREMENT * TimeError * ActualInput, m_maxRpm);
        }

        private void DampingWithEngineShaft()
        {
            switch (m_currentState)
            {
                case State.On:
                    if (m_engineRpm >= IDLING - 1)
                    {
                        m_engineRpm = TLab.Math.LinerApproach(m_engineRpm, RPM_ATTENUATION * TimeError, IDLING - 1);
                    }
                    else
                    {
                        m_engineRpm = IDLING - 1;
                    }
                    break;
                case State.Off:
                    float weight = 1.5f;
                    float targetRpm = 0.0f;
                    m_engineRpm = TLab.Math.LinerApproach(m_engineRpm, RPM_ATTENUATION * TimeError * weight, targetRpm);
                    break;
            }
        }

        private void DampingWithEngineBrake()
        {
            float rpmSum = 0f;
            foreach (WheelColliderSource brakeWheel in m_brakeWheels)
            {
                rpmSum += brakeWheel.DampingWithEngineBrake(m_engineRpm);
            }

            m_engineRpm = rpmSum / m_brakeWheels.Length;
        }

        private void Shift(int dir)
        {
            m_currentGearIndex += dir;
            m_currentGearIndex = Mathf.Clamp(m_currentGearIndex, 0, m_engineInfo.Gears.Length - 1);

            Gear gear = m_engineInfo.Gears[m_currentGearIndex];
            m_currentGear = gear.gear;
            m_currentGearRatio = gear.ratio;
        }

        public void UpdateShiftInput()
        {
            if (GearUpPressed && m_currentGearIndex < m_engineInfo.Gears.Length - 1)
            {
                Shift(1);
                GearUpPressed = false;
            }

            if (GearDownPressed && m_currentGearIndex > 0)
            {
                Shift(-1);
                GearDownPressed = false;
            }
        }

        public void SwitchEngine(State state)
        {
            m_currentState = state;
            switch (m_currentState)
            {
                case State.On:
                    m_currentGearIndex = 2;
                    break;
                case State.Off:
                    m_currentGearIndex = 1;
                    break;
            }
            m_currentGear = m_engineInfo.Gears[m_currentGearIndex].gear;
            m_currentGearRatio = m_engineInfo.Gears[m_currentGearIndex].ratio;
        }

        public void Initialize()
        {
            SwitchEngine(State.On);
            m_maxRpm = GetMaxRPM();
        }

        public void UpdateEngine()
        {
            float feedbackRPM = GetFeedbackRPM();

            m_engineRpm = GetAccelerated(feedbackRPM);

            DampingWithEngineShaft();
            DampingWithEngineBrake();

            bool transmissionConnected = !TransmissionDisconnected;
            float torque = transmissionConnected ? GetTorque() : 0.0f;

            foreach (WheelColliderSource outputDrive in m_driveWheels)
            {
                outputDrive.SetWheelState(m_engineRpm, m_currentGearRatio, torque, transmissionConnected);
            }
        }
    }
}