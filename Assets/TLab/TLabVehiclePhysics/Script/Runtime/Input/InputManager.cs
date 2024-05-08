using UnityEngine;

namespace TLab.VehiclePhysics.Input
{
    public class InputManager : MonoBehaviour
    {
        public enum InputMode
        {
            G29,
            MOUSE,
            KEYBORAD,
            UI_BUTTON
        };

        [SerializeField] private Vehicle m_vehicle;
        [SerializeField] private Engine m_engine;

        [SerializeField] private VirtualInputAxis m_virtualVertical;
        [SerializeField] private VirtualInputAxis m_virtualHorizontal;
        [SerializeField] private VirtualInputAxis m_virtualClutch;

        [SerializeField] private GameObject[] m_uiButtons;

        [SerializeField] private InputMode m_howInput;

        [SerializeField] private KeyCode m_shiftUpKey;
        [SerializeField] private KeyCode m_shiftDownKey;

        [Header("The Ackerman angle must not exceed 90.")]
        [SerializeField, Range(0f, 89f)] private float m_maxAckermannAngle = 55f;

        private float m_actualInput = 0f;
        private float m_ackermanAngle = 0f;

        private float m_accelInput = 0f;
        private float m_steerInput = 0f;
        private float m_brakeInput = 0f;
        private float m_clutchInput = 0f;

        private bool m_gearUpPressed = false;
        private bool m_gearDownPressed = false;

        private float m_shiftUpPressed = 0f;
        private float m_shiftDownPressed = 0f;

        public InputMode howInput => m_howInput;

        public float actualInput => m_actualInput;

        public float ackermanAngle => m_ackermanAngle;

        public float steerInput => m_steerInput;

        public float brakeInput => m_brakeInput;

        public float clutchInput => m_clutchInput;

        public bool gearUpPressed { get => m_gearUpPressed; set => m_gearUpPressed = value; }

        public bool gearDownPressed { get => m_gearDownPressed; set => m_gearDownPressed = value; }

        /// <summary>
        /// 
        /// </summary>
        public void ForceBreak()
        {
            m_accelInput = 0f;
            m_brakeInput = 1f;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GearUpPressedFronUIButton() => m_gearUpPressed = true;

        /// <summary>
        /// 
        /// </summary>
        public void GearDownPressedFromUIButton() => m_gearDownPressed = true;

#if !UNITY_EDITOR && UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool IsMobile();
#endif

        void Awake()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            m_howInput = IsMobile() == true ? InputMode.UI_BUTTON : m_howInput;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetVirtualInputAxis()
        {
            float rawInput = m_virtualVertical.AxisValue;
            m_accelInput = Mathf.Clamp(rawInput, 0, 1);
            m_brakeInput = Mathf.Clamp(-rawInput, 0, 1);
            m_steerInput = m_virtualHorizontal.AxisValue;
            m_clutchInput = m_virtualClutch.AxisValue;
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetShiftChangeEvent()
        {
            m_gearUpPressed = UnityEngine.Input.GetKeyDown(m_shiftUpKey);
            m_gearDownPressed = UnityEngine.Input.GetKeyDown(m_shiftDownKey);
        }

        /// <summary>
        /// 
        /// </summary>
        private void PilotIsAI()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void PilotIsPlayer()
        {
            switch (m_howInput)
            {
                case InputMode.G29:
                    if (!LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_SPRING))
                    {
                        // Set the handle to force toward 0 radians
                        LogitechGSDK.LogiPlaySpringForce(0, 0, 30, 100);
                    }

                    if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                    {
                        LogitechGSDK.DIJOYSTATE2ENGINES rec = LogitechGSDK.LogiGetStateUnity(0);

                        // 32768 cannot be represented by int,
                        // so cast to float
                        m_steerInput = rec.lX / 32768f;
                        m_accelInput = (-rec.lY / 32768f + 1) * 0.5f;
                        m_brakeInput = (-rec.lRz / 32768f + 1) * 0.5f;
                        m_clutchInput = (-rec.rglSlider[0] / 32768f + 1) * 0.5f;
                        m_shiftUpPressed = rec.rgbButtons[4];
                        m_shiftDownPressed = rec.rgbButtons[5];
                    }

                    //
                    // Provides the same behavior as GetButtonDown()
                    //

                    if (m_shiftUpPressed == 128 && m_gearUpPressed == false)
                    {
                        m_gearUpPressed = true;
                    }
                    else if (m_shiftUpPressed == 0)
                    {
                        m_gearUpPressed = false;
                    }

                    if (m_shiftDownPressed == 128 && m_gearDownPressed == false)
                    {
                        m_gearDownPressed = true;
                    }
                    else if (m_shiftDownPressed == 0)
                    {
                        m_gearDownPressed = false;
                    }
                    break;
                case InputMode.KEYBORAD:
                    GetVirtualInputAxis();
                    GetShiftChangeEvent();
                    break;
                case InputMode.MOUSE:
                    var mousePos = UnityEngine.Input.mousePosition;
                    m_accelInput = Mathf.Clamp01((mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f));
                    m_brakeInput = Mathf.Clamp01(-(mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f));
                    m_steerInput = (mousePos.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
                    m_clutchInput = m_virtualClutch.AxisValue;
                    GetShiftChangeEvent();
                    break;
                case InputMode.UI_BUTTON:
                    GetVirtualInputAxis();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        private void UIButtonSetActive(bool active)
        {
            foreach (GameObject uiButton in m_uiButtons)
            {
                uiButton.SetActive(active);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeWithInputMode()
        {
            switch (m_howInput)
            {
                case InputMode.G29:
                    Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
                    UIButtonSetActive(false);
                    break;
                case InputMode.MOUSE:
                    UIButtonSetActive(false);
                    break;
                case InputMode.KEYBORAD:
                    UIButtonSetActive(false);
                    break;
                case InputMode.UI_BUTTON:
                    UIButtonSetActive(true);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeWithPilot()
        {
            switch (m_vehicle.CurrentPilot)
            {
                case Vehicle.Pilot.None:
                    break;
                case Vehicle.Pilot.AI:
                    break;
                case Vehicle.Pilot.Player:
                    InitializeWithInputMode();
                    break;
            }
        }

        void Start()
        {
            InitializeWithPilot();
        }

        void Update()
        {
            //
            // Input processing should be described in Update
            // 1: https://unity-yuji.xyz/input-fixedupdate/
            // 2: https://qiita.com/yuji_yasuhara/items/6f50ecdd5d59e83aac99
            //

            switch (m_engine.state)
            {
                case Engine.State.ON:
                    switch (m_vehicle.CurrentPilot)
                    {
                        case Vehicle.Pilot.None:
                            break;
                        case Vehicle.Pilot.AI:
                            PilotIsAI();
                            break;
                        case Vehicle.Pilot.Player:
                            PilotIsPlayer();
                            break;
                    }

                    if (m_engine.info.transmission == EngineInfo.Transmission.AT)
                    {
                        m_clutchInput = 0f;
                    }

                    m_actualInput = m_accelInput;
                    m_ackermanAngle = m_steerInput * m_maxAckermannAngle;
                    break;
                case Engine.State.OFF:
                    m_actualInput = 0.0f;
                    m_brakeInput = 1.0f;
                    m_clutchInput = 0.0f;
                    break;
            }
        }

        void OnApplicationQuit()
        {
            switch (m_howInput)
            {
                case InputMode.G29:
                    Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
                    break;
                case InputMode.KEYBORAD:
                    break;
                case InputMode.MOUSE:
                    break;
                case InputMode.UI_BUTTON:
                    break;
            }
        }
    }
}