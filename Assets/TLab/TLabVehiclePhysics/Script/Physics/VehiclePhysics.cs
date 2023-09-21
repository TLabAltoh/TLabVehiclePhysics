using UnityEngine;

namespace TLab.VehiclePhysics
{
    public class VehiclePhysics : MonoBehaviour
    {
        [SerializeField] private TLabLUT m_frontDownForceCurve;
        [SerializeField] private TLabLUT m_rearDownForceCurve;

        [SerializeField] [Range(0.2f, 0.8f)] private float m_frontRatio = 0.5f;

        [SerializeField] private WheelColliderSource[] m_wheelsFront;
        [SerializeField] private WheelColliderSource[] m_wheelsRear;

        [SerializeField] private Rigidbody m_rigidbody;

        private float m_localVelZ = 0f;

        private const float MS2KMH = 3.6f;

        public float MeterPerSecondInLocal => m_localVelZ;

        public float KilometerPerHourInLocal => m_localVelZ * MS2KMH;

        private void SetGripFactor(WheelColliderSource[] wheels, float ratio)
        {
            foreach (WheelColliderSource wheel in wheels)
                wheel.SetGripFactor(ratio);
        }

        void Start()
        {
            m_rigidbody.maxAngularVelocity = Mathf.Infinity;
            m_rigidbody.inertiaTensor = new Vector3(2886.4406f, 3003.5281f, 1971.9375f);
            m_rigidbody.inertiaTensorRotation = Quaternion.identity;

            SetGripFactor(m_wheelsFront, m_frontRatio * 2.0f);
            SetGripFactor(m_wheelsRear, (1 - m_frontRatio) * 2.0f);
        }

        void Update()
        {
            m_localVelZ = transform.InverseTransformDirection(m_rigidbody.velocity).z;

            float velKmPh = Mathf.Abs(m_localVelZ) * 3.6f;
            float frontDownForce = m_frontDownForceCurve.Evaluate(velKmPh);
            float rearDownForce = m_rearDownForceCurve.Evaluate(velKmPh);

            SetGripFactor(m_wheelsFront, m_frontRatio * frontDownForce);
            SetGripFactor(m_wheelsRear, (1 - m_frontRatio) * rearDownForce);
        }
    }
}
