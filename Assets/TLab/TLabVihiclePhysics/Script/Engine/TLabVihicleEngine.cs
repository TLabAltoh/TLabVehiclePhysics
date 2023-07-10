using UnityEngine;
using TMPro;
using static TLab.Math;

public class TLabVihicleEngine : MonoBehaviour
{
    [Header("Vihicle")]
    [SerializeField] TLabWheelColliderSource[] driveWheels;
    [SerializeField] TLabWheelColliderSource[] brakeWheels;
    [SerializeField] TLabVihicleEngineInfo engineInfo;

    [Header("UI")]
    [SerializeField] RectTransform m_needle;
    [SerializeField] TextMeshProUGUI m_speed_Gear;

    [Header("Audio")]
    [SerializeField] AudioSource m_engineAudio;

    private int currentGearIndex = 2;
    private int currentGear;
    private float currentGearRatio;

    private float maxRpm;
    private float engineRpm = idling;
    private const float idling = 1400f;

    private const float m_startPosition = 125f;
    private const float m_needleDst = 250f;

    private const float rpmIncrement = 250f;
    private const float rpmAttenuation = 50f;

    private const float fixedTime = 30f;

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
            m_needle.eulerAngles = new Vector3(0f, 0f, m_startPosition - engineRpm / 10000 * m_needleDst);
            m_speed_Gear.text = "km/h : " + Mathf.CeilToInt(TLabVihiclePhysics.instance.KilometerPerHourInLocal).ToString() + "\n" + "gear : " + currentGear.ToString();
        }
    }

    private void UpdateEngineSound()
    {
        m_engineAudio.pitch = 1f + ((engineRpm - idling) / (maxRpm - idling)) * 2f;
    }

    public void UpdateEngine()
    {
        //
        // feedback rpm の取得
        //

        float rpmSum = 0f;
        foreach (TLabWheelColliderSource wheelOutput in driveWheels)
        {
            rpmSum += wheelOutput.FeedbackRpm;
        }

        float feedbackRPM = rpmSum / driveWheels.Length;

        //
        // アクセルによる回転の上昇
        //

        engineRpm = LinerApproach(feedbackRPM, rpmIncrement * Time.fixedDeltaTime * fixedTime * TLabVihicleInputManager.instance.ActualInput, maxRpm);

        //
        // エンジン軸での消耗
        //

        if (engineRpm >= idling - 1)
        {
            engineRpm = LinerApproach(engineRpm, rpmAttenuation * Time.fixedDeltaTime * fixedTime, idling - 1);
        }
        else
        {
            engineRpm = idling - 1;
        }

        //
        // ブレーキによる回転の減衰
        //

        rpmSum = 0f;
        foreach (TLabWheelColliderSource brakeWheel in brakeWheels)
        {
            rpmSum += brakeWheel.UpdateEngineRPMWithBreak(engineRpm);
        }

        engineRpm = rpmSum / brakeWheels.Length;

        UpdateEngineSound();
        UpdateTachometer();

        float torque;
        bool enableAddTorque;

        if (currentGearRatio == 0 || TLabVihicleInputManager.instance.ClutchInput > 0.5f || (engineRpm < idling && TLabVihicleInputManager.instance.ActualInput < 0.1f))
        {
            // Neutral or Clutch
            torque = 0.0f;
            enableAddTorque = false;
        }
        else
        {
            torque = engineInfo.rpmTorqueCurve.Evaluate(engineRpm) * TLabVihicleInputManager.instance.ActualInput;
            enableAddTorque = true;
        }

        foreach (TLabWheelColliderSource outputDrive in driveWheels)
        {
            outputDrive.EngineRpm = engineRpm;
            outputDrive.GearRatio = currentGearRatio;
            outputDrive.Torque = torque;
            outputDrive.EnableAddTorque = enableAddTorque;
        }
    }

    public void TLabStart()
    {
        currentGear = engineInfo.gears[currentGearIndex].gear;
        currentGearRatio = engineInfo.gears[currentGearIndex].ratio;

        float[] indexs = engineInfo.rpmTorqueCurve.indexs;
        maxRpm = indexs[indexs.Length - 1];

        m_engineAudio.Play();
    }

    public void TLabUpdate()
    {
        if (TLabVihicleInputManager.instance.GearUpPressed && currentGearIndex < engineInfo.gears.Length - 1)
        {
            Shift(1);
            TLabVihicleInputManager.instance.GearUpPressed = false;
        }

        if (TLabVihicleInputManager.instance.GearDownPressed && currentGearIndex > 0)
        {
            Shift(-1);
            TLabVihicleInputManager.instance.GearDownPressed = false;
        }
    }
}