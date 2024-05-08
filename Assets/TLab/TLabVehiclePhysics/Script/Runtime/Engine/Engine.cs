using UnityEngine;
using UnityEngine.Events;
using TLab.LUTTool;
using TLab.VehiclePhysics.Input;

namespace TLab.VehiclePhysics
{
    public class Engine : MonoBehaviour
    {
        public enum Drive
        {
            REVERSE,
            NEUTRAL,
            DRIVE
        };

        public enum State
        {
            ON,
            OFF
        };

        [Header("Wheel")]
        [SerializeField] private WheelColliderSource[] m_driveWheels;
        [SerializeField] private WheelColliderSource[] m_brakeWheels;

        [SerializeField] private EngineInfo m_engineInfo;

        [SerializeField] private AeroDynamics m_areoDynamics;

        [SerializeField] private InputManager m_inputManager;

        [SerializeField] private Vehicle m_vehicle;

        [Header("Callback")]
        [SerializeField] private UnityEvent m_onShiftChanged;

        private EngineInfo.GearInfo m_gearInfo;
        private int m_gearIndex = 2;
        private float m_shiftChangeIntervals = 0f;

        private Drive m_drive = Drive.NEUTRAL;
        private State m_state = State.OFF;

        private float m_maxEngineRpm;
        private float m_engineRpm = IDLING;

        private const float FIXED_TIME = 30f;
        private const float IDLING = 1400f;
        private const float RPM_INCREMENT = 250f;
        private const float RPM_ATTENUATION = 50f;
        private const int GEAR_INDEX_ONE = 2;
        private const int GEAR_INDEX_NEUTRAL = 1;

        public int gear => m_gearInfo.gear;

        public float gearRatio => m_gearInfo.ratio;

        public float engineRpm => m_engineRpm;

        public float maxEngineRpm => m_maxEngineRpm;

        private float timeError => Time.fixedDeltaTime * FIXED_TIME;

        private float actualInput => m_inputManager.actualInput;

        private float clutchInput => m_inputManager.clutchInput;

        public float pitch => 1f + ((m_engineRpm - IDLING) / (m_maxEngineRpm - IDLING)) * 2f;

        public bool transmissionDisConnected => (gearRatio == 0) || (clutchInput > 0.5f) || (m_engineRpm < IDLING && actualInput < 0.1f);

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

        public EngineInfo info => m_engineInfo;

        public State state => m_state;

        public Drive drive => m_drive;

        private void GetFeedback(out float feedbackEngineRpm, out float feedbackEngineRpmRatio)
        {
            float engineRpmSum = 0f, engineRpmRatioSum = 0f;
            foreach (WheelColliderSource wheelOutput in m_driveWheels)
            {
                engineRpmSum += wheelOutput.feedbackEngineRpm;
                engineRpmRatioSum += wheelOutput.feedbackEngineRpmRatio;
            }

            feedbackEngineRpm = engineRpmSum / m_driveWheels.Length;
            feedbackEngineRpmRatio = engineRpmRatioSum / m_driveWheels.Length;
        }

        private void GetTorque(float feedbackEngineRpmRatio, out float torque)
        {
            torque = !transmissionDisConnected ? m_engineInfo.torqueCurve.Evaluate(feedbackEngineRpmRatio, m_engineRpm) * actualInput : 0.0f;
        }

        private float GetMaxEngineRpm()
        {
            var lut = m_engineInfo.torqueCurve.lutDic[0].lut;

            var values = new Vector2[lut.values.Length - 1]; // skip last index (0, 20, 500, 501 <--- skip 501)

            System.Array.Copy(lut.values, values, values.Length);

            return LUT.GetMax(lut.values, 0);
        }

        private void GetAcceleratedEngineRpm(float feedbackEngineRpm, out float engineRpm)
        {
            engineRpm = TLab.Math.LinerApproach(feedbackEngineRpm, RPM_INCREMENT * timeError * actualInput, m_maxEngineRpm);
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

        private void SetDrive(int dir)
        {
            m_drive = (Drive)Mathf.Clamp((int)(m_drive + dir), (int)Drive.REVERSE, (int)Drive.DRIVE);

            switch (m_drive)
            {
                case Drive.NEUTRAL:
                    m_gearIndex = 1;
                    break;
                case Drive.DRIVE:
                    m_gearIndex = 2;
                    break;
                case Drive.REVERSE:
                    m_gearIndex = 0;
                    break;
            }

            SetShift(m_gearIndex);
        }

        public void SetShift(int index)
        {
            m_gearIndex = Mathf.Clamp(index, 0, m_engineInfo.gearInfos.Length - 1);

            m_gearInfo = m_engineInfo.gearInfos[m_gearIndex];

            m_shiftChangeIntervals = m_engineInfo.shiftChangeIntervals;

            m_onShiftChanged.Invoke();
        }

        public void MoveShift(int dir)
        {
            SetShift(m_gearIndex + dir);
        }

        public void UpdateShiftInput()
        {
            bool inputGearUp, inputGearDown;

            switch (m_engineInfo.transmission)
            {
                case EngineInfo.Transmission.AT:
                    inputGearUp = gearUpPressed && (m_gearInfo.gear < 1);
                    inputGearDown = gearDownPressed && (m_gearInfo.gear > -1);

                    if (inputGearUp)
                    {
                        SetDrive(1);
                        gearUpPressed = false;
                    }

                    if (inputGearDown)
                    {
                        SetDrive(-1);
                        gearDownPressed = false;
                    }

                    m_shiftChangeIntervals -= Time.deltaTime;

                    if (m_shiftChangeIntervals < 0)
                    {
                        m_shiftChangeIntervals = -1;

                        if (m_engineRpm > m_gearInfo.maxRpmThreshold)
                        {
                            MoveShift(1);
                        }

                        if (m_engineRpm < m_gearInfo.minRpmThreshold)
                        {
                            MoveShift(-1);
                        }
                    }
                    break;
                case EngineInfo.Transmission.MT:
                    inputGearUp = gearUpPressed && (m_gearIndex < m_engineInfo.gearInfos.Length - 1);
                    inputGearDown = gearDownPressed && (m_gearIndex > 0);

                    if (inputGearUp)
                    {
                        MoveShift(1);
                        gearUpPressed = false;
                    }

                    if (inputGearDown)
                    {
                        MoveShift(-1);
                        gearDownPressed = false;
                    }
                    break;
            }
        }

        public void SwitchEngine(State state)
        {
            m_state = state;

            switch (m_state)
            {
                case State.ON:
                    m_gearIndex = GEAR_INDEX_ONE;
                    m_drive = Drive.DRIVE;
                    break;
                case State.OFF:
                    m_gearIndex = GEAR_INDEX_NEUTRAL;
                    m_drive = Drive.NEUTRAL;
                    break;
            }

            m_gearInfo = m_engineInfo.gearInfos[m_gearIndex];
        }

        public void Initialize(State state, int gear)
        {
            SwitchEngine(state);

            SetShift(gear);

            m_maxEngineRpm = GetMaxEngineRpm();
        }

        public void UpdateEngine()
        {
            GetFeedback(out float feedbackEngineRpm, out float feedbackEngineRpmRatio);

            GetAcceleratedEngineRpm(feedbackEngineRpm, out m_engineRpm);

            EngineRpmDampingWithEngineShaft();
            EngineRpmDampingWithEngineBrake();

            GetTorque(feedbackEngineRpmRatio, out float torque);

            var driveData = new DriveData
            {
                engineRpm = m_engineRpm,
                gearRatio = gearRatio,
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