using UnityEngine;

public class TLabVihiclePhysics : MonoBehaviour
{
    [Header("Down Force Curve")]
    [SerializeField] TLabLUT m_frontDownForceCurve;
    [SerializeField] TLabLUT m_rearDownForceCurve;

    [Header("Wheight Ratio")]
    [SerializeField] float m_frontRatio = 0.5f;
    [SerializeField] float m_rearRatio = 0.5f;

    [Header("Wheels Source")]
    [SerializeField] TLabWheelColliderSource[] m_wheelsFront;
    [SerializeField] TLabWheelColliderSource[] m_wheelsRear;

    public static TLabVihiclePhysics instance;

    private Rigidbody rb;
    private float localVelZ = 0f;

    public float MeterPerSecondInLocal
    {
        get
        {
            return localVelZ;
        }
    }

    public float KilometerPerHourInLocal
    {
        get
        {
            const float msToKmh = 3.6f;
            return localVelZ * msToKmh;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.maxAngularVelocity = Mathf.Infinity;
        rb.inertiaTensor = new Vector3(2886.4406f, 3003.5281f, 1971.9375f);
        rb.inertiaTensorRotation = Quaternion.identity;

        foreach (TLabWheelColliderSource frontWheel in m_wheelsFront)
            frontWheel.GripFactor = m_frontRatio * 2f;

        foreach (TLabWheelColliderSource rearWheel in m_wheelsRear)
            rearWheel.GripFactor = m_rearRatio * 2f;
    }

    void Update()
    {
        localVelZ = transform.InverseTransformDirection(rb.velocity).z;

        float velKmPh = Mathf.Abs(localVelZ) * 3.6f;
        float frontDownForce = m_frontDownForceCurve.Evaluate(velKmPh);
        float rearDownForce = m_rearDownForceCurve.Evaluate(velKmPh);

        foreach (TLabWheelColliderSource frontWheel in m_wheelsFront)
            frontWheel.GripFactor = m_frontRatio * 2f * frontDownForce;

        foreach (TLabWheelColliderSource rearWheel in m_wheelsRear)
            rearWheel.GripFactor = m_rearRatio * 2f * rearDownForce;
    }
}
