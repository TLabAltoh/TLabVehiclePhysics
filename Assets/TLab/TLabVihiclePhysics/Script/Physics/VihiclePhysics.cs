using UnityEngine;

namespace TLab.VihiclePhysics
{
    public class VihiclePhysics : MonoBehaviour
    {
        [Header("Down Force Curve")]
        [SerializeField] TLabLUT m_frontDownForceCurve;
        [SerializeField] TLabLUT m_rearDownForceCurve;

        [Header("Wheight Ratio")]
        [SerializeField] [Range(0.2f, 0.8f)] float m_frontRatio = 0.5f;

        [Header("Wheels Source")]
        [SerializeField] WheelColliderSource[] m_wheelsFront;
        [SerializeField] WheelColliderSource[] m_wheelsRear;

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

        private void SetGripFactor(WheelColliderSource[] wheels, float ratio)
        {
            foreach (WheelColliderSource wheel in wheels)
                wheel.GripFactor = ratio * 2f;
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();

            rb.maxAngularVelocity = Mathf.Infinity;
            rb.inertiaTensor = new Vector3(2886.4406f, 3003.5281f, 1971.9375f);
            rb.inertiaTensorRotation = Quaternion.identity;

            SetGripFactor(m_wheelsFront, m_frontRatio);
            SetGripFactor(m_wheelsRear, 1 - m_frontRatio);
        }

        void Update()
        {
            localVelZ = transform.InverseTransformDirection(rb.velocity).z;

            float velKmPh = Mathf.Abs(localVelZ) * 3.6f;
            float frontDownForce = m_frontDownForceCurve.Evaluate(velKmPh);
            float rearDownForce = m_rearDownForceCurve.Evaluate(velKmPh);

            SetGripFactor(m_wheelsFront, m_frontRatio * frontDownForce);
            SetGripFactor(m_wheelsRear, (1 - m_frontRatio) * rearDownForce);
        }
    }
}
