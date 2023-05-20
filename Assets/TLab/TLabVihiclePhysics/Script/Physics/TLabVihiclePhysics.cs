using UnityEngine;

public class TLabVihiclePhysics : MonoBehaviour
{
    [SerializeField] TLabLUT downForceCurve;

    [Header("Wheels Source")]
    [SerializeField] TLabWheelColliderSource[] m_wheelsFront;
    [SerializeField] TLabWheelColliderSource[] m_wheelsRear;

    public static TLabVihiclePhysics instance;

    private Rigidbody rb;
    private float downForce = 1f;
    private float localVelZ = 0f;

    public float DownForce
    {
        get
        {
            return downForce;
        }
    }

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

        // 本来，前輪と後輪で重心が寄っている方がタイヤに強い重力がかかり前輪に重心が寄っているときはオーバーステア特性が出現する．
        // しかしこのスクリプトではRigidbodyの重心をどこに設定してもタイヤの重力は一定で変化しないので，再生時にここであらかじめ重力比を雑に計算しておく．

        float ratioFront = 1f;
        float ratioRear = 1f;

        foreach (TLabWheelColliderSource frontWheel in m_wheelsFront)
            frontWheel.GripFactor = ratioFront;

        foreach (TLabWheelColliderSource rearWheel in m_wheelsRear)
            rearWheel.GripFactor = ratioRear;
    }

    void Update()
    {
        localVelZ = transform.InverseTransformDirection(rb.velocity).z;
        downForce = downForceCurve.Evaluate(Mathf.Abs(localVelZ) * 3.6f);
    }
}
