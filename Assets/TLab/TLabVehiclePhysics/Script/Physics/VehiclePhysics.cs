using UnityEngine;

namespace TLab.VehiclePhysics
{
    public class VehiclePhysics : MonoBehaviour
    {
        [SerializeField] private MultiLUT m_frontDownForceCurve;
        [SerializeField] private MultiLUT m_rearDownForceCurve;

        [SerializeField] [Range(0.2f, 0.8f)]
        private float m_frontRatio = 0.5f;

        [SerializeField] private WheelColliderSource[] m_wheelsFront;
        [SerializeField] private WheelColliderSource[] m_wheelsRear;

        [SerializeField] private Rigidbody m_rigidbody;

        private float m_angle = 0.0f;

        private float m_localVelZ = 0.0f;

        private const float MS2KMH = 3.6f;

        /// <summary>
        /// Tilt of the z-direction of the vehicle body and the speed of the vehicle body (degree)
        /// </summary>
        public float angle => m_angle;

        /// <summary>
        /// 
        /// </summary>
        public float minAngle => 180f - m_angle < m_angle ? 180f - m_angle : m_angle;

        /// <summary>
        /// Speed of car body relative to local axis of car body (m/s)
        /// </summary>
        public float meterPerSecondInLocal => m_localVelZ;

        /// <summary>
        /// Speed of car body relative to local axis of car body (km/h)
        /// </summary>
        public float kilometerPerHourInLocal => m_localVelZ * MS2KMH;

        private void SetGripFactor(WheelColliderSource[] wheels, float ratio)
        {
            foreach (WheelColliderSource wheel in wheels)
            {
                wheel.SetGripFactor(ratio);
            }
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
            var velocity = Vector3.ProjectOnPlane(m_rigidbody.velocity, transform.up);
            m_angle = Vector3.Angle(transform.forward, velocity.normalized);
            m_localVelZ = transform.InverseTransformDirection(velocity).z;

            var angle = minAngle;
            var velKmPh = Mathf.Abs(m_localVelZ) * MS2KMH;
            var rearDownForce = m_rearDownForceCurve.Evaluate(angle, velKmPh);
            var frontDownForce = m_frontDownForceCurve.Evaluate(angle, velKmPh);

            SetGripFactor(m_wheelsFront, m_frontRatio * 2f * frontDownForce);
            SetGripFactor(m_wheelsRear, (1 - m_frontRatio) * 2f * rearDownForce);
        }
    }
}
