using UnityEngine;

public class TLabVihicleSystemManager : MonoBehaviour
{
    [Header("Handle Animation")]
    [SerializeField] Transform handleTransform;
    [SerializeField] float rotAngleOfHandle = 100f;

    [Header("Vihicle")]
    [SerializeField] TLabVihicleEngine m_engine;
    [SerializeField] TLabWheelColliderSource[] m_wheelColliderSources;

    public static TLabVihicleSystemManager Instance;

    private void UpdateHandleRotation()
    {
        Vector3 HandleTransformlocalEuler = handleTransform.localEulerAngles;
        HandleTransformlocalEuler.z = TLabVihicleInputManager.instance.SteerInput * rotAngleOfHandle;
        handleTransform.localEulerAngles = HandleTransformlocalEuler;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        foreach (TLabWheelColliderSource wheelColliderSource in m_wheelColliderSources)
            wheelColliderSource.TLabStart();
        m_engine.TLabStart();
    }

    public void Update()
    {
        m_engine.TLabUpdate();
    }

    public void FixedUpdate()
    {
        m_engine.UpdateEngine();

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
