using UnityEngine;
using TMPro;
using static TLab.Math;

namespace TLab.VehiclePhysics
{
    public class VehicleEngine : MonoBehaviour
    {
        [Header("Wheel Collider Source")]
        [SerializeField] WheelColliderSource[] driveWheels;
        [SerializeField] WheelColliderSource[] brakeWheels;

        [Header("Engine Info")]
        [SerializeField] VehicleEngineInfo engineInfo;

        [Header("Vehicle Physics")]
        [SerializeField] VehiclePhysics vihiclePhysics;

        [Header("Input")]
        [SerializeField] VehicleInputManager inputManager;

        [Header("UI")]
        [SerializeField] RectTransform m_needle;
        [SerializeField] TextMeshProUGUI m_speed_Gear;

        [Header("Audio")]
        [SerializeField] AudioSource m_engineAudio;

        private int currentGearIndex = 2;
        private int currentGear;
        private float currentGearRatio;

        private bool engineActive = false;

        private float maxRpm;
        private float engineRpm = IDLING;
        private const float IDLING = 1400f;

        private const float NEEDLE_START = 125f;
        private const float NEEDLE_DST = 250f;

        private const float RPMINCREMENT = 250f;
        private const float RPMATTENUATION = 50f;

        private const float FIXEDTIME = 30f;

        public int CurrentGear
        {
            get
            {
                return currentGear;
            }
        }

        public float EngineRpm
        {
            get
            {
                return engineRpm;
            }
        }

        public float EngineMaxRpm
        {
            get
            {
                return maxRpm;
            }
        }

        private float GetTimeError
        {
            get
            {
                return Time.fixedDeltaTime * FIXEDTIME;
            }
        }

        private float GetActualInput
        {
            get
            {
                return inputManager.ActualInput;
            }
        }

        private float GetClutchInput
        {
            get
            {
                return inputManager.ClutchInput;
            }
        }

        private bool IsNeutralOrClutch
        {
            get
            {
                return currentGearRatio == 0 || GetClutchInput > 0.5f || (engineRpm < IDLING && GetActualInput < 0.1f);
            }
        }

        private bool GearUpPressed
        {
            get
            {
                return inputManager.GearUpPressed;
            }

            set
            {
                inputManager.GearUpPressed = value;
            }
        }

        private bool GearDownPressed
        {
            get
            {
                return inputManager.GearDownPressed;
            }

            set
            {
                inputManager.GearDownPressed = value;
            }
        }

        private int CurrentKPH
        {
            get
            {
                return Mathf.CeilToInt(vihiclePhysics.KilometerPerHourInLocal);
            }
        }

        public bool EngineActive
        {
            get
            {
                return engineActive;
            }
        }

        private void Shift(int dir)
        {
            currentGearIndex += dir;
            currentGearIndex = Mathf.Clamp(currentGearIndex, 0, engineInfo.gears.Length - 1);
            currentGear = engineInfo.gears[currentGearIndex].gear;
            currentGearRatio = engineInfo.gears[currentGearIndex].ratio;
        }

        private void UpdateTachometer()
        {
            if (m_needle.gameObject.activeInHierarchy == true)
            {
                m_needle.eulerAngles = new Vector3(0f, 0f, NEEDLE_START - engineRpm / 10000 * NEEDLE_DST);
                m_speed_Gear.text = "km/h : " + CurrentKPH.ToString() + "\n" + "gear : " + currentGear.ToString();
            }
        }

        private void UpdateEngineSound()
        {
            m_engineAudio.pitch = 1f + ((engineRpm - IDLING) / (maxRpm - IDLING)) * 2f;
        }

        private float GetFeedbackRPM()
        {
            float rpmSum = 0f;
            foreach (WheelColliderSource wheelOutput in driveWheels)
            {
                rpmSum += wheelOutput.FeedbackRpm;
            }

            float feedbackRPM = rpmSum / driveWheels.Length;

            return feedbackRPM;
        }

        private float Accelerator(float feedbackRPM)
        {
            return LinerApproach(feedbackRPM, RPMINCREMENT * GetTimeError * GetActualInput, maxRpm);
        }

        #region Damping
        private void DampingAtEngineShaft()
        {
            if (engineActive)
            {
                if (engineRpm >= IDLING - 1)
                    engineRpm = LinerApproach(engineRpm, RPMATTENUATION * GetTimeError, IDLING - 1);
                else
                    engineRpm = IDLING - 1;
            }
            else
            {
                engineRpm = LinerApproach(engineRpm, RPMATTENUATION * GetTimeError * 1.5f, 0);
            }
        }

        private void DampingAtEngineBrake()
        {
            float rpmSum = 0f;
            foreach (WheelColliderSource brakeWheel in brakeWheels)
                rpmSum += brakeWheel.UpdateEngineRPMWithBreak(engineRpm);

            engineRpm = rpmSum / brakeWheels.Length;
        }
        #endregion Damping

        private float GetNextTorque()
        {
            return engineInfo.rpmTorqueCurve.Evaluate(engineRpm) * GetActualInput;
        }

        public void UpdateEngine()
        {
            float feedbackRPM = GetFeedbackRPM();

            engineRpm = Accelerator(feedbackRPM);

            DampingAtEngineShaft();
            DampingAtEngineBrake();

            UpdateEngineSound();
            UpdateTachometer();

            float torque;
            bool enableAddTorque;

            if (IsNeutralOrClutch)
            {
                torque = 0.0f;
                enableAddTorque = false;
            }
            else
            {
                torque = GetNextTorque();
                enableAddTorque = true;
            }

            foreach (WheelColliderSource outputDrive in driveWheels)
            {
                outputDrive.EngineRpm = engineRpm;
                outputDrive.GearRatio = currentGearRatio;
                outputDrive.Torque = torque;
                outputDrive.EnableAddTorque = enableAddTorque;
            }
        }

        private void SetMaxRPM()
        {
            float[] indexs = engineInfo.rpmTorqueCurve.indexs;
            maxRpm = indexs[indexs.Length - 1];
        }

        private void SwitchEngineAudio(bool active)
        {
            if (active)
                m_engineAudio.Play();
            else
                m_engineAudio.Stop();
        }

        public void SwitchEngine(bool active)
        {
            currentGearIndex = active ? 2 : 1;
            currentGear = engineInfo.gears[currentGearIndex].gear;
            currentGearRatio = engineInfo.gears[currentGearIndex].ratio;

            engineActive = active;
        }

        public void TLabStart()
        {
            SwitchEngine(true);
            SetMaxRPM();
            SwitchEngineAudio(true);
        }

        public void TLabUpdate()
        {
            if (GearUpPressed && currentGearIndex < engineInfo.gears.Length - 1)
            {
                Shift(1);
                GearUpPressed = false;
            }

            if (GearDownPressed && currentGearIndex > 0)
            {
                Shift(-1);
                GearDownPressed = false;
            }
        }
    }
}