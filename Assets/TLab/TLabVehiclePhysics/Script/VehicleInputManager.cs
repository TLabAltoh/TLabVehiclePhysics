using System;
using UnityEngine;

namespace TLab.VehiclePhysics
{
    public enum InputMode
    {
        InputFromG29,
        InputFromMouse,
        InputFromKeyBord,
        InputFromVirtualUI
    };

    public class VehicleInputManager : MonoBehaviour
    {
        // G29 takes 0 for all items only in the first frame, so it accelerates (this can't be helped)

        [Header("Vehicle Physics Manager")]
        [SerializeField] VehicleSystemManager systemManager;

        [Header("Keyborad")]
        [SerializeField] string clucth = "CarInput Clucth";
        [SerializeField] string upGear = "CarInput Up Shift";
        [SerializeField] string downGear = "CarInput Down Shift";

        [Header("Mobile")]
        [SerializeField] VirtualInput virtualVertical;
        [SerializeField] VirtualInput virtualHorizontal;
        [SerializeField] VirtualInput virtualClutch;
        [SerializeField] GameObject virtualInputGroup;

        [Header("HowInput")]
        [SerializeField] InputMode howInput;

        private Action axisInput;
        private Action shiftInput;

        private float actualInput = 0f;
        private float ackermanAngle = 0f;

        private float accelInput = 0f;
        private float steerInput = 0f;
        private float brakeInput = 0f;
        private float clutchInput = 0f;

        private bool gearUpPressed = false;
        private bool gearDownPressed = false;

        private float shiftUpPressed = 0f;
        private float shiftDownPressed = 0f;

        public InputMode HowInput
        {
            get
            {
                return howInput;
            }
        }

        public float ActualInput
        {
            get
            {
                return actualInput;
            }
        }

        public float AckermanAngle
        {
            get
            {
                return ackermanAngle;
            }
        }

        public float SteerInput
        {
            get
            {
                return steerInput;
            }
        }

        public float BrakeInput
        {
            get
            {
                return brakeInput;
            }
        }

        public float ClutchInput
        {
            get
            {
                return clutchInput;
            }
        }

        public bool GearUpPressed
        {
            get
            {
                return gearUpPressed;
            }

            set
            {
                gearUpPressed = value;
            }
        }

        public bool GearDownPressed
        {
            get
            {
                return gearDownPressed;
            }

            set
            {
                gearDownPressed = value;
            }
        }

        public void ForceBreak()
        {
            accelInput = 0f;
            brakeInput = 1f;
        }

        private void InputFromG29()
        {
            if (!LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_SPRING))
            {
                // Set the handle to force toward 0 radians
                LogitechGSDK.LogiPlaySpringForce(0, 0, 30, 100);
            }

            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                LogitechGSDK.DIJOYSTATE2ENGINES rec = LogitechGSDK.LogiGetStateUnity(0);

                // 32768 cannot be represented by int, so cast to float
                steerInput = rec.lX / 32768f;
                accelInput = (-rec.lY / 32768f + 1) * 0.5f;
                brakeInput = (-rec.lRz / 32768f + 1) * 0.5f;
                clutchInput = (-rec.rglSlider[0] / 32768f + 1) * 0.5f;
                shiftUpPressed = rec.rgbButtons[4];
                shiftDownPressed = rec.rgbButtons[5];
            }
        }


        private void InputFromMouse()
        {
            Vector3 mousePos = Input.mousePosition;
            accelInput = Mathf.Clamp01((mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f));
            brakeInput = Mathf.Clamp01(-(mousePos.y - Screen.height * 0.5f) / (Screen.height * 0.5f));
            steerInput = (mousePos.x - Screen.width * 0.5f) / (Screen.width * 0.5f);
            if (!string.IsNullOrEmpty(clucth))
                clutchInput = Input.GetAxis(clucth);
        }


        private void InputFromKeyBord()
        {
            float rawInput = Input.GetAxis("Vertical");
            accelInput = Mathf.Clamp(rawInput, 0, 1);
            brakeInput = Mathf.Clamp(-rawInput, 0, 1);
            steerInput = Input.GetAxis("Horizontal");
            if (!string.IsNullOrEmpty(clucth))
                clutchInput = Input.GetAxis(clucth);
        }

        private void InputFromVirtualUI()
        {
            float rawInput = virtualVertical.InputValue;
            accelInput = Mathf.Clamp(rawInput, 0, 1);
            brakeInput = Mathf.Clamp(-rawInput, 0, 1);
            steerInput = virtualHorizontal.InputValue;
            clutchInput = virtualClutch.InputValue;
        }


        private void ShiftPressedFromG29()
        {
            // Provides the same behavior as GetButtonDown()

            if (shiftUpPressed == 128 && gearUpPressed == false)
                gearUpPressed = true;
            else if (shiftUpPressed == 0)
                gearUpPressed = false;

            if (shiftDownPressed == 128 && gearDownPressed == false)
                gearDownPressed = true;
            else if (shiftDownPressed == 0)
                gearDownPressed = false;
        }


        private void ShiftPressedFromKey()
        {
            // Run only on the frame at the moment of pressing
            if (!string.IsNullOrEmpty(upGear))
                gearUpPressed = Input.GetButtonDown(upGear);

            if (!string.IsNullOrEmpty(downGear))
                gearDownPressed = Input.GetButtonDown(downGear);
        }

        private void ShiftPressedFromVirtualUI()
        {
            //
        }

        public void GearUpPressedFromUI()
        {
            gearUpPressed = true;
        }

        public void GearUpDownFromUI()
        {
            gearDownPressed = true;
        }

        private void SetActualInput()
        {
            actualInput = accelInput;
            ackermanAngle = steerInput * 55f;
        }

#if !UNITY_EDITOR && UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool IsMobile();
#endif

        void Awake()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        howInput = IsMobile() == true ? InputMode.InputFromVirtualUI : howInput;
#endif
        }

        void Start()
        {
            virtualInputGroup.SetActive(false);

            switch (howInput)
            {
                case (InputMode.InputFromG29):
                    // Initialization function of logitech sdk (must be executed)
                    Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
                    axisInput = InputFromG29;
                    shiftInput = ShiftPressedFromG29;
                    break;
                case (InputMode.InputFromMouse):
                    axisInput = InputFromMouse;
                    shiftInput = ShiftPressedFromKey;
                    break;
                case (InputMode.InputFromKeyBord):
                    axisInput = InputFromKeyBord;
                    shiftInput = ShiftPressedFromKey;
                    break;
                case (InputMode.InputFromVirtualUI):
                    virtualInputGroup.SetActive(true);
                    axisInput = InputFromVirtualUI;
                    shiftInput = ShiftPressedFromVirtualUI;
                    break;
            }
        }

        void Update()
        {
            // Disable vehicle control inputs when the player exits the vehicle
            if (systemManager.GettingOff == true)
                return;

            // Input processing should be described in Update
            // https://unity-yuji.xyz/input-fixedupdate/
            // https://qiita.com/yuji_yasuhara/items/6f50ecdd5d59e83aac99

            axisInput();
            shiftInput();
            SetActualInput();
        }

        void OnApplicationQuit()
        {
            // Run on exit (destroy controller object
            if (howInput == InputMode.InputFromG29)
                Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
        }
    }
}