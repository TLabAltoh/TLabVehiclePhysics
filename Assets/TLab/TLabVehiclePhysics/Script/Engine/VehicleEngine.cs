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

        public int currentGear => m_currentGear;

        public float engineRpm => m_engineRpm;

        public float maxRpm => m_maxRpm;

        private float timeError => Time.fixedDeltaTime * FIXED_TIME;

        private float actualInput => m_inputManager.actualInput;

        private float clutchInput => m_inputManager.clutchInput;

        public float pitch => 1f + ((m_engineRpm - IDLING) / (m_maxRpm - IDLING)) * 2f;

        public bool transmissionDisConnected => m_currentGearRatio == 0 || clutchInput > 0.5f || (m_engineRpm < IDLING && actualInput < 0.1f);

        private bool gearUpPressed
        {
            get => m_inputManager.gearUpPressed;
            set => m_inputManager.gearUpPressed = value;
        }

        private bool gearDownPressed
        {
            get => m_inputManager.gearDownPressed;
            set => m_inputManager.gearDownPressed = value;
        }

        public State CurrentState => m_currentState;

        private float GetFeedbackRPM()
        {
            float rpmSum = 0f;
            foreach (WheelColliderSource wheelOutput in m_driveWheels)
            {
                rpmSum += wheelOutput.feedbackRpm;
            }

            float feedbackRPM = rpmSum / m_driveWheels.Length;

            return feedbackRPM;
        }

        private float GetTorque() => m_engineInfo.torqueCurve.Evaluate(m_engineRpm) * actualInput;

        private float GetMaxRPM() => LUT.GetMax(m_engineInfo.torqueCurve.values, 0);

        private float GetAccelerated(float feedbackRPM) => TLab.Math.LinerApproach(feedbackRPM, RPM_INCREMENT * timeError * actualInput, m_maxRpm);

        private void DampingWithEngineShaft()
        {
            switch (m_currentState)
            {
                case State.On:
                    if (m_engineRpm >= IDLING - 1)
                    {
                        m_engineRpm = TLab.Math.LinerApproach(m_engineRpm, RPM_ATTENUATION * timeError, IDLING - 1);
                    }
                    else
                    {
                        m_engineRpm = IDLING - 1;
                    }
                    break;
                case State.Off:
                    m_engineRpm = TLab.Math.LinerApproach(m_engineRpm, RPM_ATTENUATION * timeError * 1.5f, 0.0f);
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
            if (gearUpPressed && m_currentGearIndex < m_engineInfo.Gears.Length - 1)
            {
                Shift(1);
                gearUpPressed = false;
            }

            if (gearDownPressed && m_currentGearIndex > 0)
            {
                Shift(-1);
                gearDownPressed = false;
            }
        }

        public void SwitchEngine(State state)
        {
            m_currentState = state;
            switch (m_currentState)
            {
                case State.On:
                    m_currentGearIndex = 2; // gear 1
                    break;
                case State.Off:
                    m_currentGearIndex = 1; // gear 0 (neutral)
                    break;
            }
            m_currentGear = m_engineInfo.Gears[m_currentGearIndex].gear;
            m_currentGearRatio = m_engineInfo.Gears[m_currentGearIndex].ratio;
        }

        public void Initialize()
        {
            SwitchEngine(State.On);
            m_maxRpm = GetMaxRPM();
            Debug.Log("maxRpm: " + m_maxRpm);
        }

        public void UpdateEngine()
        {
            float feedbackRPM = GetFeedbackRPM();

            m_engineRpm = GetAccelerated(feedbackRPM);

            //Debug.Log("engineRpm: " + m_engineRpm);

            DampingWithEngineShaft();
            DampingWithEngineBrake();

            float torque = !transmissionDisConnected ? GetTorque() : 0.0f;

            var driveData = new DriveData
            {
                engineRpm = m_engineRpm,
                gearRatio = m_currentGearRatio,
                torque = torque,
                transmissionConnected = !transmissionDisConnected
            };

            foreach (WheelColliderSource outputDrive in m_driveWheels)
            {
                outputDrive.SetDriveData(driveData);
            }
        }
    }
}