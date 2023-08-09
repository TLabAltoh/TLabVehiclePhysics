using UnityEngine;

public class TLabVihicleSystemManager : MonoBehaviour
{
    [Header("Handle Animation")]
    [SerializeField] Transform handleTransform;
    [SerializeField] float rotAngleOfHandle = 100f;

    [Header("Engine")]
    [SerializeField] TLabVihicleEngine engine;

    [Header("InputManager")]
    [SerializeField] TLabVihicleInputManager inputManager;

    [Header("Wheels")]
    [SerializeField] TLabWheelColliderSource[] m_wheelColliderSources;

    // Whether the player is getting out of the vehicle
    private bool m_gettingOff = false;

    /// <summary>
    /// When enabled, the vehicle operation inputs (accelerator,brake, etc...) and CarCameraInputManager are disabled.
    /// </summary>
    public bool GettingOff
    {
        get
        {
            return m_gettingOff;
        }

        set
        {
            m_gettingOff = value;
        }
    }

    private void UpdateHandleRotation()
    {
        Vector3 handleLocalEuler = handleTransform.localEulerAngles;
        handleLocalEuler.z = inputManager.SteerInput * rotAngleOfHandle;
        handleTransform.localEulerAngles = handleLocalEuler;
    }

    public void Start()
    {
        foreach (TLabWheelColliderSource wheelColliderSource in m_wheelColliderSources)
            wheelColliderSource.TLabStart();

        engine.TLabStart();
    }

    public void Update()
    {
        engine.TLabUpdate();
    }

    public void FixedUpdate()
    {
        engine.UpdateEngine();

        for (int i = 0; i < m_wheelColliderSources.Length; i++)
        {
            TLabWheelColliderSource wheelColliderSource = m_wheelColliderSources[i];
            wheelColliderSource.UpdateSuspension();
            wheelColliderSource.UpdateWheel();
            wheelColliderSource.WheelAddForce();
            wheelColliderSource.UpdateEngineRpm();
        }

        UpdateHandleRotation();
    }
}
