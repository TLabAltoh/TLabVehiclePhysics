using UnityEngine;
using static TLab.ComponentUtility;

namespace TLab.VehiclePhysics
{
    public class WheelColliderSource : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask m_layer;

        [Header("Wheel Option")]
        [SerializeField] private bool m_steerEnabled = true;
        [SerializeField] private bool m_driveEnabled = false;

        [Header("Engine")]
        [SerializeField] private VehicleEngine m_engine;

        [Header("Physics")]
        [SerializeField] private WheelPhysics m_wheelPhysics;
        [SerializeField] private WheelColliderSource m_arbPear;

        [Header("Input")]
        [SerializeField] private VehicleInputManager m_inputManager;

        [Header("Wheel Debugger Material")]
        [SerializeField] private Material m_lineMaterial;

        private SphereCollider m_collider;
        private Transform m_dummyWheel;
        private Rigidbody m_rigidbody;
        private RaycastHit m_raycastHit;

        private DriveData m_driveData = new DriveData();    // not null allowed

        private float m_wheelRotationY = 0f;
        private float m_rawWheelRpm = 0f;
        private float m_wheelRpm = 0f;
        private float m_slipRatio = 0f;
        private float m_slipAngle = 0f;
        private float m_downforce = 1f;
        private float m_feedbackEngineRpm = 0f;
        private float m_feedbackEngineRpmRatio = 0f;
        private float m_endPointTorque = 0f;
        private float m_endPointGearRatio = 0f;

        private const float WEIGHT_RATIO = 0.25f;
        private const float DIFFGEAR_RATIO = 3.42f;
        private const float IDLING = 1400f;
        private const float FIXED_TIME = 30f;

        public float feedbackSlipAngle => m_slipAngle;

        public float feedbackSlipRatio => m_slipRatio;

        public float feedbackEngineRpm => m_feedbackEngineRpm;

        public float feedbackEngineRpmRatio => m_feedbackEngineRpmRatio;

        public float finalTorque => m_endPointTorque;

        public float finalGearRatio => m_endPointGearRatio;

        public float rawWheelRpm => m_rawWheelRpm;

        public float wheelRpm => m_wheelRpm;

        /**
         * input axis
         */

        private float actualInput => m_inputManager.actualInput;

        private float brakeInput => m_inputManager.brakeInput;

        private float clutchInput => m_inputManager.clutchInput;

        private float ackermanAngle => m_steerEnabled ? m_inputManager.ackermanAngle : 0f;

        /// <summary>
        /// The maximum amount of rotation of the wheel, determined from the engine speed and gear ratio.
        /// </summary>
        public float maxWheelRpm => m_endPointGearRatio != 0 ? m_engine.maxEngineRpm / Mathf.Abs(m_endPointGearRatio) : Mathf.Infinity;

        /// <summary>
        /// Grip coefficient dependent on downforce
        /// </summary>
        public float downforce => m_downforce;

        public Bounds bounds => new Bounds(transform.position, Vector3.one * m_wheelPhysics.wheelRadius * 2.0f);

        public void SetDownForce(float factor) => m_downforce = factor;

        private float GetFrameLerp(float value) => value * FIXED_TIME * Time.fixedDeltaTime;

        public void SetDriveData(DriveData driveData)
        {
            m_driveData = driveData;

            m_endPointGearRatio = driveData.gearRatio * DIFFGEAR_RATIO;
            m_endPointTorque = driveData.torque * m_endPointGearRatio / m_wheelPhysics.wheelRadius;
        }

        public void DrawWheel(Camera camera)
        {
            if (!m_lineMaterial)
            {
                Debug.LogError("line material is null");
                return;
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

            if (!GeometryUtility.TestPlanesAABB(planes, bounds))
            {
                return;
            }

            m_lineMaterial.color = m_wheelPhysics.gizmoColor;
            m_lineMaterial.SetPass(0);

            const float LINE_WIDTH = 0.0025f;

            const int DIM = 20;

            GLUtil.ClearVertex();
            {
                GLUtil.AddVertex(transform.position - m_dummyWheel.up * m_wheelPhysics.wheelRadius);
                GLUtil.AddVertex(transform.position + (m_dummyWheel.up * (m_wheelPhysics.susDst - m_wheelPhysics.susCps)));
                GLUtil.DrawLinesWidth(camera, LINE_WIDTH, DIM);
            }

            GLUtil.CreateTrigonometricTable(DIM, out float[] sin, out float[] cos);

            Vector3[] points = new Vector3[DIM];

            Vector3 susVec, offset = transform.TransformVector(Vector3.right * 0.1f);

            for (int i = 0; i < DIM; i++)
            {
                susVec = new Vector3(0, sin[i], cos[i]);
                points[i] = transform.TransformPoint(m_wheelPhysics.wheelRadius * susVec);
            }

            GLUtil.ClearVertex();
            {
                for (int i = 0; i < DIM; i++)
                {
                    GLUtil.AddVertex(points[i] + offset);
                }

                GLUtil.DrawLineWidth(camera, LINE_WIDTH, true, DIM);
            }

            GLUtil.ClearVertex();
            {
                for (int i = 0; i < DIM; i++)
                {
                    GLUtil.AddVertex(points[i] - offset);
                }

                GLUtil.DrawLineWidth(camera, LINE_WIDTH, true, DIM);
            }

            GLUtil.ClearVertex();
            {
                for (int i = 0; i < DIM; i++)
                {
                    GLUtil.AddVertex(points[i] + offset);   // Connect Left and Right
                    GLUtil.AddVertex(points[i] - offset);
                }

                GLUtil.DrawLinesWidth(camera, LINE_WIDTH, DIM);
            }
        }

        private void UpdateSuspension()
        {
            var grounded = Physics.Raycast(new Ray(m_dummyWheel.position, -m_dummyWheel.up), out m_raycastHit, m_wheelPhysics.wheelRadius + m_wheelPhysics.susDst, m_layer);

            transform.localPosition = m_wheelPhysics.UpdateSuspention(m_raycastHit, m_dummyWheel, grounded);
        }

        private void UpdateWheelRot()
        {
            /**
             * Dummy wheel
             */
            m_dummyWheel.localEulerAngles = new Vector3(0f, ackermanAngle, 0f);

            /**
             * Visual wheel
             */
            m_wheelRotationY += m_wheelRpm / 60 * 360 * Time.fixedDeltaTime;
            transform.localEulerAngles = new Vector3(m_wheelRotationY, ackermanAngle, 0f);
        }

        /// <summary>
        /// Vehicle velocity in world space (m/s)
        /// </summary>
        public Vector3 velocity => m_rigidbody.GetPointVelocity(m_dummyWheel.transform.position);

        /// <summary>
        /// Vehicle velocity in wheel local space (m/s)
        /// </summary>
        public Vector3 localVelocity => m_dummyWheel.transform.InverseTransformDirection(velocity);

        /// <summary>
        /// Wheel anguler velocity (m/s)
        /// </summary>
        public float angularVelocity => m_wheelRpm * m_wheelPhysics.wheelRpm2Vel;

        private void WheelAddForce()
        {
            if (!m_wheelPhysics.grounded)
            {
                return;
            }

            var wheelLocalVelocity = localVelocity;

            var velXDir = -System.Math.Sign(wheelLocalVelocity.x);

            var velZDir = -System.Math.Sign(wheelLocalVelocity.z);

            var vel2WheelRPM = wheelLocalVelocity.z * m_wheelPhysics.vel2WheelRpm;

            var achieveMaxWheelRPM = Mathf.Abs(vel2WheelRPM) >= maxWheelRpm;

            m_slipAngle = 0f;

            if (velZDir == 0)   // Prevents division-by-zero errors
            {
                /**
                 * wheelLocalVelocity.z = 0 : already established
                 * if wheelLocalVelocity.x = 0 --> slipAngle = 0
                 */
                m_slipAngle = System.Math.Sign(wheelLocalVelocity.x) * 90f * Mathf.Deg2Rad;
            }
            else
            {
                /**
                 * Calculate slip angle with inverse tangent (return value in radians)
                 */
                m_slipAngle = Mathf.Atan(wheelLocalVelocity.x / wheelLocalVelocity.z);
            }

            /**
             * Limit not to exceed maxWheelRpm (engine brake).
             */
            m_rawWheelRpm = achieveMaxWheelRPM ? Mathf.Sign(vel2WheelRPM) * maxWheelRpm : vel2WheelRPM;

            var rawAngularVelocity = (m_rawWheelRpm * m_wheelPhysics.wheelRpm2Vel);

            var absRawAngularVelocity = Mathf.Abs(rawAngularVelocity);

            var slipZ = angularVelocity - rawAngularVelocity;

            var absSlipZ = Mathf.Abs(slipZ);

            m_slipRatio = (absRawAngularVelocity > 0.1f) ? (absSlipZ / absRawAngularVelocity) : (absSlipZ > 0.1f ? 1f : 0f);

            var slipAmount = Mathf.Sqrt(
                slipZ * slipZ + // Forward slip
                wheelLocalVelocity.x * wheelLocalVelocity.x // Horizontal slip
                );

            /**
             * Calculate frictional force ---> ---->
             */

            /**
             * Gravity on the tire
             */
            var gravity = (m_rigidbody.mass * WEIGHT_RATIO + m_wheelPhysics.wheelMass) * 9.8f;

            /**
             * Rolling resistance (0.015 is the magic number to estimate rolling resistance)
             */
            var rollingResistance = velZDir * gravity * 0.015f;

            /**
             * Estimation of frictional force
             */
            var baseGrip = m_wheelPhysics.baseGrip.Evaluate(slipAmount);

            /**
             * Grip by Slip Ratio
             */

            var absSlipRatio = Mathf.Abs(m_slipRatio);

            var slipRatioLateralGrip = m_wheelPhysics.slipRatioVsLateralGrip.Evaluate(absSlipRatio);

            var slipRatioLongitudinalGrip = m_wheelPhysics.slipRatioVsLongitudinalGrip.Evaluate(absSlipRatio);

            /**
             * Longitudinal force
             */

            var longitudinalGrip = baseGrip * slipRatioLongitudinalGrip * m_downforce;

            var longitudinalForce = velZDir * gravity * longitudinalGrip;

            /**
             * Lateral force
             */

            var slipAngleGrip = m_wheelPhysics.slipAngleVsGrip.Evaluate(Mathf.Abs(m_slipAngle) * Mathf.Rad2Deg);

            var lateralGrip = baseGrip * slipAngleGrip * slipRatioLateralGrip * m_downforce;

            var lateralForce = velXDir * gravity * lateralGrip;

            /**
             * Vectoring frictional forces ---> --->
             */

            var targetX = lateralForce;

            var targetZ = rollingResistance;

            if (brakeInput > 0.1f || achieveMaxWheelRPM)
            {
                targetZ = targetZ + Mathf.Cos(m_slipAngle) * longitudinalForce;
            }
            else if (m_driveData.transmissionConnected)
            {
                targetZ = targetZ + m_endPointTorque;
            }

            var suspentionForce = m_raycastHit.normal * m_wheelPhysics.suspentionFource;
            var totalTireForce = m_rigidbody.transform.TransformDirection(targetX, 0f, targetZ);

            m_rigidbody.AddForceAtPosition(totalTireForce, m_dummyWheel.position, ForceMode.Force);
            m_rigidbody.AddForceAtPosition(suspentionForce, transform.position, ForceMode.Force);
        }

        private void UpdateEngineRpm()
        {
            if (!m_wheelPhysics.grounded)
            {
                m_rawWheelRpm = m_wheelRpm;
            }

            if (m_driveEnabled)
            {
                if (m_driveData.transmissionConnected)
                {
                    const float INCREMENT = 500f, DECREMENT = 25f;

                    /**
                     * Tires are more likely to slip when the car is inclined with respect to the direction of travel
                     */
                    var rawEngineRpm = Mathf.Abs(m_rawWheelRpm * m_endPointGearRatio);
                    var torqueRatio = m_wheelPhysics.torqueVsRpmLerpRatio.Evaluate(Mathf.Abs(m_endPointTorque));
                    var angleRatio = m_wheelPhysics.slipAngleVsRpmLerpRatio.Evaluate(Mathf.Abs(m_slipAngle * Mathf.Rad2Deg));
                    var toRawEngineRpm = Math.LinerApproach(m_driveData.engineRpm, GetFrameLerp(INCREMENT) * angleRatio * torqueRatio, rawEngineRpm);

                    /**
                     * Update current tire rotation amount.
                     */
                    var wheelRpm = Mathf.Sign(m_rawWheelRpm) * toRawEngineRpm / Mathf.Abs(m_endPointGearRatio);
                    m_wheelRpm = wheelRpm;

                    /**
                     * Damping of engine rpm due to transfer of engine power to gears
                     */
                    var feedbackEngineRpm = Math.LinerApproach(toRawEngineRpm, GetFrameLerp(DECREMENT), IDLING - 1);
                    m_feedbackEngineRpm = feedbackEngineRpm;
                    m_feedbackEngineRpmRatio = Mathf.Abs(rawEngineRpm / m_engine.maxEngineRpm);
                }
                else
                {
                    const float DECREMENT = 1.0f, TARGET_RPM = 0.0f;

                    /**
                     * Because the transmission is not connected, the amount of tire rotation is dragged by the inertia of the vehicle body.
                     */

                    m_feedbackEngineRpm = m_driveData.engineRpm;
                    m_feedbackEngineRpmRatio = 0f;

                    m_wheelRpm = Math.LinerApproach(m_rawWheelRpm, GetFrameLerp(DECREMENT), TARGET_RPM);
                }
            }
            else
            {
                const float DECREMENT = 1.0f, TARGET_RPM = 0.0f;

                /**
                 * Because the transmission is not connected, the amount of tire rotation is dragged by the inertia of the vehicle body.
                 */
                m_wheelRpm = Math.LinerApproach(m_rawWheelRpm, GetFrameLerp(DECREMENT), TARGET_RPM);
            }
        }

        public float EngineRpmDampingWithEngineBrake(float engineRPM)
        {
            if (m_driveData.transmissionConnected)
            {
                const float DECREMENT = 2500f;

                var lerpedEngineRpm = Math.LinerApproach(engineRPM, GetFrameLerp(brakeInput * DECREMENT), 0);

                return lerpedEngineRpm > IDLING ? lerpedEngineRpm : IDLING - 1;
            }
            else
            {
                const float DECREMENT = 2500f;

                /**
                 * Since the engine cannot be braked, the tires are locked directly.
                 */
                m_wheelRpm = Math.LinerApproach(m_wheelRpm, GetFrameLerp(brakeInput * DECREMENT), 0);

                return engineRPM;
            }
        }

        public void UpdateWheel()
        {
            UpdateSuspension();
            UpdateWheelRot();
            WheelAddForce();
            UpdateEngineRpm();
        }

        public void Initialize()
        {
            /**
             * For debugging tires
             */
            m_lineMaterial = new Material(m_lineMaterial);

            m_dummyWheel = new GameObject("DummyWheel").transform;
            m_dummyWheel.transform.position = this.transform.position;
            m_dummyWheel.transform.parent = this.transform.parent;

            m_rigidbody = GetComponentFromParent<Rigidbody>(this.transform, this.GetType().FullName.ToString());

            /**
             * Add a collider to enable raycast
             */
            m_collider = gameObject.AddComponent<SphereCollider>();
            m_collider.center = new Vector3(0.0f, m_wheelPhysics.wheelRadius, 0.0f);
            m_collider.radius = 0.0f;
        }
    }
}