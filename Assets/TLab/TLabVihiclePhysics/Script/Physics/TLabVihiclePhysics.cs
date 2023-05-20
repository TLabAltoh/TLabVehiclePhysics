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

        // �{���C�O�ւƌ�ւŏd�S������Ă�������^�C���ɋ����d�͂�������O�ւɏd�S������Ă���Ƃ��̓I�[�o�[�X�e�A�������o������D
        // ���������̃X�N���v�g�ł�Rigidbody�̏d�S���ǂ��ɐݒ肵�Ă��^�C���̏d�͈͂��ŕω����Ȃ��̂ŁC�Đ����ɂ����ł��炩���ߏd�͔���G�Ɍv�Z���Ă����D

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
