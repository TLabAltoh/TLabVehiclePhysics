using UnityEngine;
using static TLab.ComponentUtility;

namespace TLab.VehiclePhysics
{
    public class WheelColliderSource : MonoBehaviour
    {
        [SerializeField] private LayerMask m_layer;
        [SerializeField] private bool m_steerEnabled = true;
        [SerializeField] private bool m_driveEnabled = false;
        [Space(10)]
        [SerializeField] private VehicleEngine m_engine;
        [SerializeField] private VehicleInputManager m_inputManager;
        [SerializeField] private WheelPhysics m_wheelPhysics;
        [SerializeField] private WheelColliderSource m_arbPear;
        [Space(10)]
        [SerializeField] private Material m_lineMaterial;

        private SphereCollider m_collider;
        private Transform m_dummyWheel;
        private Rigidbody m_rigidbody;
        private RaycastHit m_raycastHit;

        private DriveData m_driveData = new DriveData();    // not null allowed

        private float m_wheelRot = 0f;
        private float m_rawWheelRPM = 0f;
        private float m_currentWheelRpm = 0f;
        private float m_slipAngle = 0f;
        private float m_gripFactor = 1f;
        private float m_totalGrip = 1f;
        private float m_frictionForce = 0f;
        private float m_feedbackRpm = 0f;
        private float m_finalTorque = 0f;
        private float m_finalGearRatio = 0f;

        private const float WEIGHT_RATIO = 0.25f;
        private const float DIFFGEAR_RATIO = 3.42f;
        private const float IDLING = 1400f;
        private const float FIXED_TIME = 30f;

        public float feedbackRpm => m_feedbackRpm;

        public float totalGrip => m_totalGrip;

        public float finalTorque => m_finalTorque;

        public float finalGearRatio => m_finalGearRatio;

        // input axis

        private float actualInput => m_inputManager.actualInput;

        private float brakeInput => m_inputManager.brakeInput;

        private float clutchInput => m_inputManager.clutchInput;

        private float ackermanAngle => m_steerEnabled ? m_inputManager.ackermanAngle : 0f;

        /// <summary>
        /// The maximum amount of rotation of the wheel, determined from the engine speed and gear ratio.
        /// </summary>
        public float maxWheelRpm => m_finalGearRatio != 0 ? m_engine.maxRpm / Mathf.Abs(m_finalGearRatio) : Mathf.Infinity;

        public void SetGripFactor(float factor) => m_gripFactor = factor;

        private float GetFrameLerp(float value) => value * FIXED_TIME * Time.fixedDeltaTime;

        public void SetDriveData(DriveData driveData)
        {
            m_driveData = driveData;

            m_finalGearRatio = driveData.gearRatio * DIFFGEAR_RATIO;
            m_finalTorque = driveData.torque * m_finalGearRatio / m_wheelPhysics.wheelRadius;
        }

        public void DrawWheel()
        {
            if (!m_lineMaterial)
            {
                Debug.Log("line material is null");
                return;
            }

            GL.PushMatrix();
            {
                m_lineMaterial.color = m_wheelPhysics.gizmoColor;
                m_lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                {
                    GL.Vertex(transform.position - m_dummyWheel.up * m_wheelPhysics.wheelRadius);
                    GL.Vertex(transform.position + (m_dummyWheel.up * (m_wheelPhysics.susDst - m_wheelPhysics.susCps)));
                }
                GL.End();

                m_lineMaterial.color = m_wheelPhysics.gizmoColor;
                m_lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                {
                    float tempsin = Mathf.Sin(0 / 20f * Mathf.PI * 2);
                    float tempcos = Mathf.Cos(0 / 20f * Mathf.PI * 2);

                    Vector3 susVec = new Vector3(0, tempsin, tempcos);

                    Vector3 offset = transform.TransformVector(Vector3.right * 0.1f);
                    Vector3 point = transform.TransformPoint(m_wheelPhysics.wheelRadius * susVec);

                    // Connect Left and Right
                    GL.Vertex(point + offset);
                    GL.Vertex(point - offset);

                    Vector3 prevPoint = point;

                    for (int i = 0; i < 20; ++i)
                    {
                        tempsin = Mathf.Sin(i / 20f * Mathf.PI * 2);
                        tempcos = Mathf.Cos(i / 20f * Mathf.PI * 2);
                        susVec = new Vector3(0, tempsin, tempcos);
                        point = transform.TransformPoint(m_wheelPhysics.wheelRadius * susVec);

                        // Right Side
                        GL.Vertex(prevPoint + offset);
                        GL.Vertex(point + offset);

                        // Left Side
                        GL.Vertex(prevPoint - offset);
                        GL.Vertex(point - offset);

                        // Connect Left and Right
                        GL.Vertex(point + offset);
                        GL.Vertex(point - offset);

                        prevPoint = point;
                    }

                    tempsin = Mathf.Sin(Mathf.PI * 2);
                    tempcos = Mathf.Cos(Mathf.PI * 2);
                    susVec = new Vector3(0, tempsin, tempcos);
                    point = transform.TransformPoint(m_wheelPhysics.wheelRadius * susVec);

                    // Right Side
                    GL.Vertex(prevPoint + offset);
                    GL.Vertex(point + offset);

                    // Left Side
                    GL.Vertex(prevPoint - offset);
                    GL.Vertex(point - offset);
                }
                GL.End();
            }
            GL.PopMatrix();
        }

        private void UpdateSuspension()
        {
            var grounded = Physics.Raycast(new Ray(m_dummyWheel.position, -m_dummyWheel.up), out m_raycastHit, m_wheelPhysics.wheelRadius + m_wheelPhysics.susDst, m_layer);

            transform.localPosition = m_wheelPhysics.UpdateSuspention(m_raycastHit, m_dummyWheel, grounded);
        }

        private void UpdateWheelRot()
        {
            // Dummy wheel
            m_dummyWheel.localEulerAngles = new Vector3(0f, ackermanAngle, 0f);

            // Visual wheel
            m_wheelRot += m_currentWheelRpm / 60 * 360 * Time.fixedDeltaTime;
            transform.localEulerAngles = new Vector3(m_wheelRot, ackermanAngle, 0f);
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
        public float angularVelocity => m_currentWheelRpm * m_wheelPhysics.wheelRpm2Vel;

        private void WheelAddForce()
        {
            if (!m_wheelPhysics.grounded)
            {
                return;
            }

            var wheelLocalVelocity = localVelocity;

            var velZDir = -System.Math.Sign(wheelLocalVelocity.z);

            var vel2WheelRPM = wheelLocalVelocity.z * m_wheelPhysics.vel2WheelRpm;

            var achieveMaxRPM = Mathf.Abs(vel2WheelRPM) >= maxWheelRpm;

            // Limit not to exceed maxWheelRpm (engine brake).
            m_rawWheelRPM = achieveMaxRPM ? Mathf.Sign(vel2WheelRPM) * maxWheelRpm : vel2WheelRPM;

            var slipZ = angularVelocity - (m_rawWheelRPM * m_wheelPhysics.wheelRpm2Vel);
            var slipAmount = Mathf.Sqrt(
                slipZ * slipZ + /* Forward slip */
                wheelLocalVelocity.x * wheelLocalVelocity.x /* Horizontal slip */);

            var slipRatio = Mathf.Abs(wheelLocalVelocity.z) > 0.1f ? slipZ / wheelLocalVelocity.z : 2f;

            m_slipAngle = 0f;

            // Prevents division-by-zero errors
            if (velZDir == 0)
            {
                // wheelLocalVelocity.z = 0 : already established
                // if wheelLocalVelocity.x = 0 --> slipAngle = 0
                m_slipAngle = System.Math.Sign(wheelLocalVelocity.x) * 90f * Mathf.Deg2Rad;
            }
            else
            {
                // Calculate slip angle with inverse tangent (return value in radians)
                m_slipAngle = Mathf.Atan(wheelLocalVelocity.x / wheelLocalVelocity.z);
            }

            // Calculate frictional force ---> ---->

            // Gravity on the tire
            var gravity = (m_rigidbody.mass * WEIGHT_RATIO + m_wheelPhysics.wheelMass) * 9.8f;

            // Rolling resistance (0.015 is the magic number to estimate rolling resistance)
            var rollingResistance = velZDir * gravity * 0.015f;

            // Estimation of frictional force
            var baseGrip = m_wheelPhysics.baseGrip.Evaluate(slipAmount);
            var slipRatioGrip = m_wheelPhysics.slipRatioVsGrip.Evaluate(Mathf.Abs(slipRatio));

            m_totalGrip = baseGrip * slipRatioGrip * m_gripFactor;

            m_frictionForce = velZDir * gravity * m_totalGrip;

            //Debug.Log("baseGrip: " + baseGrip + "slipRatio: " + slipRatio + "slipRatioGrip: " + slipRatioGrip);
            //Debug.Log("transmission connected: " + m_driveData.transmissionConnected);
            //Debug.Log("friction force: " + m_frictionForce);

            // Vectoring frictional forces ---> --->

            var targetX = Mathf.Sin(m_slipAngle) * m_frictionForce;

            var targetZ = rollingResistance;

            if (brakeInput > 0.1f || achieveMaxRPM)
            {
                Debug.Log("achieveMaxRPM or brakeInput");
                targetZ = targetZ + Mathf.Cos(m_slipAngle) * m_frictionForce;
            }
            else if (m_driveData.transmissionConnected)
            {
                //Debug.Log("transmission connected ! " + this.gameObject.name);
                //Debug.Log("final torque: " + m_finalTorque);
                targetZ = targetZ + m_finalTorque;
            }

            var suspentionForce = m_raycastHit.normal * m_wheelPhysics.suspentionFource;
            var totalTireForce = m_dummyWheel.transform.TransformDirection(targetX, 0f, targetZ);

            m_rigidbody.AddForceAtPosition(totalTireForce, m_dummyWheel.position, ForceMode.Force);
            m_rigidbody.AddForceAtPosition(suspentionForce, transform.position, ForceMode.Force);
        }

        private void UpdateEngineRpm()
        {
            if (!m_wheelPhysics.grounded)
            {
                m_rawWheelRPM = m_currentWheelRpm;
            }

            if (m_driveEnabled)
            {
                if (m_driveData.transmissionConnected)
                {
                    const float increment = 2000f;

                    // Tires are more likely to slip when the car is inclined with respect to the direction of travel
                    var dst = Mathf.Abs(m_rawWheelRPM * m_finalGearRatio);
                    var torqueRatio = m_wheelPhysics.torqueVsLerpRatio.Evaluate(Mathf.Abs(m_finalTorque));
                    var angleRatio = m_wheelPhysics.slipAngleVsLerpRatio.Evaluate(Mathf.Abs(m_slipAngle / Mathf.PI * 180));
                    var toRawEngineRpm = TLab.Math.LinerApproach(m_driveData.engineRpm, GetFrameLerp(increment) * angleRatio * torqueRatio, dst);

                    //Debug.Log("torque: " + torqueRatio + " angle: " + angleRatio);
                    //Debug.Log("slip angle: " + m_slipAngle);
                    //Debug.Log("toraw: " + toRawEngineRpm);
                    Debug.Log("engin: " + m_driveData.engineRpm);

                    const float decrement = 25f;

                    // Damping of rpm due to transfer of engine power to gears
                    var attenuated = TLab.Math.LinerApproach(toRawEngineRpm, GetFrameLerp(decrement), IDLING - 1);
                    m_feedbackRpm = attenuated;

                    var wheelRpm = Mathf.Sign(m_rawWheelRPM) * attenuated / Mathf.Abs(m_finalGearRatio);
                    m_currentWheelRpm = wheelRpm;

                    //Debug.Log("feedback: " + m_feedbackRpm);
                }
                else
                {
                    const float decrement = 0.5f;
                    const float targetRpm = 0.0f;
                    m_feedbackRpm = m_driveData.engineRpm;
                    m_currentWheelRpm = TLab.Math.LinerApproach(m_rawWheelRPM, GetFrameLerp(decrement), targetRpm);
                }
            }
            else
            {
                const float decrement = 0.5f;
                const float targetRpm = 0.0f;
                m_currentWheelRpm = TLab.Math.LinerApproach(m_rawWheelRPM, GetFrameLerp(decrement), targetRpm);
            }
        }

        public float DampingWithEngineBrake(float engineRPM)
        {
            if (m_driveData.transmissionConnected)
            {
                var lerp = TLab.Math.LinerApproach(engineRPM, GetFrameLerp(brakeInput * 50f), 0);
                m_currentWheelRpm = Mathf.Sign(m_rawWheelRPM) * lerp / Mathf.Abs(m_finalGearRatio);
                return lerp > IDLING ? lerp : IDLING - 1;
            }
            else
            {
                m_currentWheelRpm = TLab.Math.LinerApproach(m_currentWheelRpm, GetFrameLerp(brakeInput * 50f), 0);
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
            // For debugging tires
            m_lineMaterial = new Material(m_lineMaterial);

            m_dummyWheel = new GameObject("DummyWheel").transform;
            m_dummyWheel.transform.position = this.transform.position;
            m_dummyWheel.transform.parent = this.transform.parent;

            m_rigidbody = GetComponentFromParent<Rigidbody>(this.transform, this.GetType().FullName.ToString());

            // Add a collider to enable raycast
            m_collider = gameObject.AddComponent<SphereCollider>();
            m_collider.center = new Vector3(0.0f, m_wheelPhysics.wheelRadius, 0.0f);
            m_collider.radius = 0.0f;
        }
    }
}