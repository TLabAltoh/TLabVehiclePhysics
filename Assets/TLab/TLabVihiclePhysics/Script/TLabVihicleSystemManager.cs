using UnityEngine;

public class TLabVihicleSystemManager : MonoBehaviour
{
    [Header("Handle Animation")]
    [SerializeField] Transform handleTransform;
    [SerializeField] float rotAngleOfHandle = 100f;

    [Header("Wheels")]
    [SerializeField] TLabWheelColliderSource[] m_wheelColliderSources;

    public static TLabVihicleSystemManager Instance;

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
