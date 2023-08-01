using UnityEngine;

public class TLabVihicleSystemManager : MonoBehaviour
{
    [Header("Handle Animation")]
    [SerializeField] Transform handleTransform;
    [SerializeField] float rotAngleOfHandle = 100f;

    [Header("Wheels")]
    [SerializeField] TLabWheelColliderSource[] m_wheelColliderSources;

    // Whether the player is getting out of the vehicle
    private bool m_gettingOff = false;

    public static TLabVihicleSystemManager Instance;

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
        handleLocalEuler.z = TLabVihicleInputManager.instance.SteerInput * rotAngleOfHandle;
        handleTransform.localEulerAngles = handleLocalEuler;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        foreach (TLabWheelColliderSource wheelColliderSource in m_wheelColliderSources)
            wheelColliderSource.TLabStart();

        TLabVihicleEngine.Instance.TLabStart();
    }

    public void Update()
    {
        TLabVihicleEngine.Instance.TLabUpdate();
    }

    public void FixedUpdate()
    {
        TLabVihicleEngine.Instance.UpdateEngine();

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
