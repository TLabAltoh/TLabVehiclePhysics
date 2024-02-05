using UnityEngine;

namespace TLab.VehiclePhysics
{
    public class VehicleEngine : MonoBehaviour
    {
        public enum State
        {
            ON,
            OFF
        }

        [SerializeField] private WheelColliderSource[] m_driveWheels;
        [SerializeField] private WheelColliderSource[] m_brakeWheels;

        [Space(10)]
        [SerializeField] private VehicleEngineInfo m_engineInfo;

        [Space(10)]
        [SerializeField] private VehicleSystemManager m_systemManager;
        [SerializeField] private VehiclePhysics m_vihiclePhysics;
        [SerializeField] private VehicleInputManager m_inputManager;

        private int m_gearIndex = 2;
        private int m_gear;
        private float m_gearRatio;

        private State m_state = State.OFF;

        private float m_maxEngineRpm;
        private float m_engineRpm = IDLING;
        private float m_rawEngineRpmRatio;

        private const float FIXED_TIME = 30f;
        private const float IDLING = 1400f;
        private const float RPM_INCREMENT = 250f;
        private const float RPM_ATTENUATION = 50f;
        private const int GEAR_INDEX_ONE = 2;
        private const int GEAR_INDEX_NEUTRAL = 1;

        public int gear => m_gear;

        public float engineRpm => m_engineRpm;

        public float maxEngineRpm => m_maxEngineRpm;

        private float timeError => Time.fixedDeltaTime * FIXED_TIME;

        private float actualInput => m_inputManager.actualInput;

        private float clutchInput => m_inputManager.clutchInput;

        public float pitch => 1f + ((m_engineRpm - IDLING) / (m_maxEngineRpm - IDLING)) * 2f;

        public bool transmissionDisConnected => m_gearRatio == 0 || clutchInput > 0.5f || (m_engineRpm < IDLING && actualInput < 0.1f);

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

        public State state => m_state;

        private (float, float) GetFeedback()
        {
            float engineRpmSum = 0f, engineRpmRatioSum = 0f;
            foreach (WheelColliderSource wheelOutput in m_driveWheels)
            {
                engineRpmSum += wheelOutput.feedbackEngineRpm;
                engineRpmRatioSum += wheelOutput.feedbackEngineRpmRatio;
            }

            return (engineRpmSum / m_driveWheels.Length, engineRpmRatioSum / m_driveWheels.Length);
        }

        private float GetTorque(float feedbackEngineRpmRatio)
        {
            return m_engineInfo.torqueCurve.Evaluate(feedbackEngineRpmRatio, m_engineRpm) * actualInput;
        }

        private float GetMaxEngineRpm()
        {
            var lut = m_engineInfo.torqueCurve.lutDic[0].lut;

            var values = new Vector2[lut.values.Length - 1]; // skip last index (0, 20, 500, 501 <--- skip 501)

            System.Array.Copy(lut.values, values, values.Length);

            return LUT.GetMax(lut.values, 0);
        }

        private float GetAcceleratedEngineRpm(float feedbackEngineRpm)
        {
            return TLab.Math.LinerApproach(feedbackEngineRpm, RPM_INCREMENT * timeError * actualInput, m_maxEngineRpm);
        }

        private void EngineRpmDampingWithEngineShaft()
        {
            switch (m_state)
            {
                case State.ON:
                    if (m_engineRpm >= IDLING - 1)
                    {
                        m_engineRpm = TLab.Math.LinerApproach(m_engineRpm, RPM_ATTENUATION * timeError, IDLING - 1);
                    }
                    else
                    {
                        m_engineRpm = IDLING - 1;
                    }
                    break;
                case State.OFF:
                    m_engineRpm = TLab.Math.LinerApproach(m_engineRpm, RPM_ATTENUATION * timeError * 1.5f, 0.0f);
                    break;
            }
        }

        private void EngineRpmDampingWithEngineBrake()
        {
            float sum = 0f;
            foreach (WheelColliderSource brakeWheel in m_brakeWheels)
            {
                sum += brakeWheel.EngineRpmDampingWithEngineBrake(m_engineRpm);
            }

            m_engineRpm = sum / m_brakeWheels.Length;
        }

        private void Shift(int dir)
        {
            m_gearIndex = Mathf.Clamp(m_gearIndex + dir, 0, m_engineInfo.gearInfos.Length - 1);

            GearInfo gearInfo = m_engineInfo.gearInfos[m_gearIndex];
            m_gear = gearInfo.gear;
            m_gearRatio = gearInfo.ratio;
        }

        public void UpdateShiftInput()
        {
            if (gearUpPressed && m_gearIndex < m_engineInfo.gearInfos.Length - 1)
            {
                Shift(1);
                gearUpPressed = false;
            }

            if (gearDownPressed && m_gearIndex > 0)
            {
                Shift(-1);
                gearDownPressed = false;
            }
        }

        public void SwitchEngine(State state)
        {
            m_state = state;
            switch (m_state)
            {
                case State.ON:
                    m_gearIndex = GEAR_INDEX_ONE;
                    break;
                case State.OFF:
                    m_gearIndex = GEAR_INDEX_NEUTRAL;
                    break;
            }
            m_gear = m_engineInfo.gearInfos[m_gearIndex].gear;
            m_gearRatio = m_engineInfo.gearInfos[m_gearIndex].ratio;
        }

        public void Initialize()
        {
            SwitchEngine(State.ON);
            m_maxEngineRpm = GetMaxEngineRpm();
        }

        public void UpdateEngine()
        {
            (float, float) feedback = GetFeedback();

            m_engineRpm = GetAcceleratedEngineRpm(feedback.Item1);

            EngineRpmDampingWithEngineShaft();
            EngineRpmDampingWithEngineBrake();

            float torque = !transmissionDisConnected ? GetTorque(feedback.Item2) : 0.0f;

            var driveData = new DriveData
            {
                engineRpm = m_engineRpm,
                gearRatio = m_gearRatio,
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