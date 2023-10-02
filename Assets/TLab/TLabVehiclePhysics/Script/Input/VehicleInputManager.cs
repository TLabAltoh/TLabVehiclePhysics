using System;
using UnityEngine;

namespace TLab.VehiclePhysics
{
    public class VehicleInputManager : MonoBehaviour
    {
        public enum InputMode
        {
            G29,
            Mouse,
            Keyborad,
            UIButton
        };

        [SerializeField] private VehicleSystemManager m_systemManager;

        [SerializeField] private VirtualInputAxis m_virtualVertical;
        [SerializeField] private VirtualInputAxis m_virtualHorizontal;
        [SerializeField] private VirtualInputAxis m_virtualClutch;

        [SerializeField] private GameObject[] m_uiButtons;

        [SerializeField] private InputMode m_howInput;

        [SerializeField] private KeyCode m_shiftUpKey;
        [SerializeField] private KeyCode m_shiftDownKey;

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

        public InputMode HowInput => m_howInput;

        public float ActualInput => m_actualInput;

        public float AckermanAngle => m_ackermanAngle;

        public float SteerInput => m_steerInput;

        public float BrakeInput => m_brakeInput;

        public float ClutchInput => m_clutchInput;

        public bool GearUpPressed { get => m_gearUpPressed; set => m_gearUpPressed = value; }

        public bool GearDownPressed { get => m_gearDownPressed; set => m_gearDownPressed = value; }

        public void ForceBreak()
        {
            m_accelInput = 0f;
            m_brakeInput = 1f;
        }

        public void GearUpPressedFronUIButton() => m_gearUpPressed = true;

        public void GearDownPressedFromUIButton() => m_gearDownPressed = true;

#if !UNITY_EDITOR && UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool IsMobile();
#endif

        void Awake()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        m_howInput = IsMobile() == true ? InputMode.UIButton : m_howInput;
#endif
        }

        private void GetVirtualInputAxis()
        {
            float rawInput = m_virtualVertical.AxisValue;
            m_accelInput = Mathf.Clamp(rawInput, 0, 1);
            m_brakeInput = Mathf.Clamp(-rawInput, 0, 1);
            m_steerInput = m_virtualHorizontal.AxisValue;
            m_clutchInput = m_virtualClutch.AxisValue;
        }

        private void GetShiftChangeEvent()
        {
            m_gearUpPressed = Input.GetKeyDown(m_shiftUpKey);
            m_gearDownPressed = Input.GetKeyDown(m_shiftDownKey);
        }

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

                        // 32768 cannot be represented by int, so cast to float
                        m_steerInput = rec.lX / 32768f;
                        m_accelInput = (-rec.lY / 32768f + 1) * 0.5f;
                        m_brakeInput = (-rec.lRz / 32768f + 1) * 0.5f;
                        m_clutchInput = (-rec.rglSlider[0] / 32768f + 1) * 0.5f;
                        m_shiftUpPressed = rec.rgbButtons[4];
                        m_shiftDownPressed = rec.rgbButtons[5];
                    }

                    // Provides the same behavior as GetButtonDown()
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
                case InputMode.Keyborad:
                    GetVirtualInputAxis();
                    GetShiftChangeEvent();
                    break;
                case InputMode.Mouse:
                    Vector3 mousePos = Input.mousePosition;
                    m_accelInput = Mathf.Clamp01((mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f));
                    m_brakeInput = Mathf.Clamp01(-(mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f));
                    m_steerInput = (mousePos.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
                    m_clutchInput = m_virtualClutch.AxisValue;

                    GetShiftChangeEvent();
                    break;
                case InputMode.UIButton:
                    GetVirtualInputAxis();

                    break;
            }
        }

        private void UIButtonSetActive(bool active)
        {
            foreach (GameObject uiButton in m_uiButtons)
            {
                uiButton.SetActive(active);
            }
        }

        private void InitializeWithInputMode()
        {
            switch (m_howInput)
            {
                case InputMode.G29:
                    Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
                    UIButtonSetActive(false);
                    break;
                case InputMode.Mouse:
                    UIButtonSetActive(false);
                    break;
                case InputMode.Keyborad:
                    UIButtonSetActive(false);
                    break;
                case InputMode.UIButton:
                    UIButtonSetActive(true);
                    break;
            }
        }

        private void InitializeWithPilot()
        {
            switch (m_systemManager.CurrentPilot)
            {
                case VehicleSystemManager.Pilot.None:
                    //
                    break;
                case VehicleSystemManager.Pilot.AI:
                    //
                    break;
                case VehicleSystemManager.Pilot.Player:
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
            switch (m_systemManager.CurrentPilot)
            {
                case VehicleSystemManager.Pilot.None:
                    break;
                case VehicleSystemManager.Pilot.AI:
                    break;
                case VehicleSystemManager.Pilot.Player:
                    PilotIsPlayer();
                    break;
            }

            // Input processing should be described in Update
            // https://unity-yuji.xyz/input-fixedupdate/
            // https://qiita.com/yuji_yasuhara/items/6f50ecdd5d59e83aac99

            m_actualInput = m_accelInput;
            m_ackermanAngle = m_steerInput * 55f;
        }

        void OnApplicationQuit()
        {
            switch (m_howInput)
            {
                case InputMode.G29:
                    Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
                    break;
                case InputMode.Keyborad:
                    break;
                case InputMode.Mouse:
                    break;
                case InputMode.UIButton:
                    break;
            }
        }
    }
}