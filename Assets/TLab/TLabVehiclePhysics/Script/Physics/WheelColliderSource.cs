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
        private float m_wheelRot = 0f;
        private float m_rawWheelRPM = 0f;
        private float m_currentWheelRpm = 0f;
        private float m_slipAngle = 0f;
        private float m_gripFactor = 1f;
        private float m_totalGrip = 1f;
        private float m_engineRpm = 0f;
        private float m_feedbackRpm = 0f;
        private float m_torque = 0f;
        private float m_gearRatio = 0f;
        private float m_totalGearRatio = 0f;
        private float m_circleLength;
        private float m_susCps = 0f;
        private float m_susCpsPrev = 0f;
        private bool m_transmissionConnected = false;
        private bool m_grounded = false;
        private Color m_gizmoColor = Color.green;

        private const float WEIGHT_RATIO = 0.25f;
        private const float DIFFGEAR_RATIO = 3.42f;
        private const float IDLING = 1400f;
        private const float FIXED_TIME = 30f;

        // feedback

        public float FeedbackRpm => m_feedbackRpm;

        public float TotalGrip => m_totalGrip;

        // input axis

        private float ActualInput => m_inputManager.ActualInput;

        private float BrakeInput => m_inputManager.BrakeInput;

        private float AckermanAngle => m_inputManager.AckermanAngle;

        private float ClutchInput => m_inputManager.ClutchInput;

        //

        private float WheelRPM2Vel => m_circleLength / 60;

        private float Vel2WheelRPM => 60 / m_circleLength;

        //

        private float GuessedEngineRpm => m_rawWheelRPM * m_totalGearRatio;

        private float MaxWheelRPM
        {
            get
            {
                if (m_totalGearRatio != 0)
                {
                    return m_engine.EngineMaxRpm / Mathf.Abs(m_totalGearRatio);
                }
                else
                {
                    return Mathf.Infinity;
                }
            }
        }

        //

        private bool TransmissionConnected => m_transmissionConnected || GuessedEngineRpm >= IDLING && ClutchInput <= 0.5f;

        public void SetGripFactor(float factor) => m_gripFactor = factor;

        private float GetFrameLerp(float value) => value * FIXED_TIME * Time.fixedDeltaTime;

        public void SetWheelState(float engineRpm, float gearRatio, float torque, bool transmissionConnected)
        {
            m_engineRpm = engineRpm;
            m_gearRatio = gearRatio;
            m_totalGearRatio = m_gearRatio * DIFFGEAR_RATIO;
            m_torque = torque * m_totalGearRatio / m_wheelPhysics.wheelRadius;
            m_transmissionConnected = transmissionConnected;
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
                m_lineMaterial.color = m_gizmoColor;
                m_lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                {
                    GL.Vertex(transform.position - m_dummyWheel.up * m_wheelPhysics.wheelRadius);
                    GL.Vertex(transform.position + (m_dummyWheel.up * (m_wheelPhysics.susDst - m_susCps)));
                }
                GL.End();

                m_lineMaterial.color = m_gizmoColor;
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
            m_grounded = Physics.Raycast(new Ray(m_dummyWheel.position, -m_dummyWheel.up), out m_raycastHit, m_wheelPhysics.wheelRadius + m_wheelPhysics.susDst, m_layer);

            if (m_grounded)
            {
                m_gizmoColor = Color.green;
                m_susCpsPrev = m_susCps;

                var stretchedOut = m_wheelPhysics.wheelRadius + m_wheelPhysics.susDst;
                var suspentionOrigin = m_dummyWheel.position;
                m_susCps = stretchedOut - (m_raycastHit.point - suspentionOrigin).magnitude;
            }
            else
            {
                m_gizmoColor = Color.blue;
                m_susCps = 0f;
            }

            var suspentionLocalOrigin = m_dummyWheel.localPosition;
            var amountOfReduction = Vector3.up * (m_wheelPhysics.susDst - m_susCps);
            transform.localPosition = suspentionLocalOrigin - amountOfReduction;
        }

        private void UpdateWheelRot()
        {
            var ackermanAngle = m_steerEnabled ? AckermanAngle : 0;

            // dummy wheel
            Vector3 dummyRot = m_dummyWheel.localEulerAngles;
            dummyRot.x = 0f;
            dummyRot.y = ackermanAngle;
            dummyRot.z = 0f;
            m_dummyWheel.localEulerAngles = dummyRot;

            // visual wheel
            m_wheelRot += m_currentWheelRpm / 60 * 360 * Time.fixedDeltaTime;
            Vector3 visualRot = transform.localEulerAngles;
            visualRot.x = m_wheelRot;
            visualRot.y = ackermanAngle;
            visualRot.z = 0f;
            transform.localEulerAngles = visualRot;
        }

        private void WheelAddForce()
        {
            if (m_grounded == false)
            {
                return;
            }

            // �^�C���̃^�C�����Ƀ��[�J���Ȉړ����x (m/s)
            var wheelVelocity = m_rigidbody.GetPointVelocity(m_dummyWheel.transform.position);
            var wheelLocalVelocity = m_dummyWheel.transform.InverseTransformDirection(wheelVelocity);

            // �^�C���̉�]�̑��x���Z (m/s)
            var wheelAngularVelocity = m_currentWheelRpm * WheelRPM2Vel;

            // �^�C���̑��x�̉�]�����Z (rpm)
            var vel2WheelRPM = wheelLocalVelocity.z * Vel2WheelRPM;
            var achieveMaxRPM = Mathf.Abs(vel2WheelRPM) >= MaxWheelRPM;

            // vel2WheelRPM�� MaxWheelRPM�����鎞�� m_rawWheelRPM��MaxWheelRPM�ɐ������ăG���W���u���[�L�𔭐�������
            m_rawWheelRPM = achieveMaxRPM ? Mathf.Sign(vel2WheelRPM) * MaxWheelRPM : vel2WheelRPM;

            // �X���b�v�̎����l (m/s)
            var slipZ = wheelAngularVelocity - (m_rawWheelRPM * WheelRPM2Vel);
            var slipSqrZ = slipZ * slipZ;
            var slipSqrX = wheelLocalVelocity.x * wheelLocalVelocity.x;
            var slipAmount = Mathf.Sqrt(slipSqrX + slipSqrZ);

            // �X���b�v��
            var slipRatio = Mathf.Abs(wheelLocalVelocity.z) > 0.1f ? slipZ / wheelLocalVelocity.z : 2f;

            // �X���b�v�A���O�����t�^���W�F���g�Ōv�Z
            m_slipAngle = 0.0f;

            // ���x�̌���
            var velZDir = -System.Math.Sign(wheelLocalVelocity.z);

            // 0���Z�G���[��h�~
            if (velZDir == 0)
            {
                // wheelLocalVelocity.z = 0 : ���łɐ���
                // wheelLocalVelocity.x = 0 : ��������������� --> m_slipAngle = 0
                m_slipAngle = System.Math.Sign(wheelLocalVelocity.x) * 90 * Mathf.Deg2Rad;
            }
            else
            {
                // �t�^���W�F���g�ŃX���b�v�A���O�����v�Z(�Ԃ�l�̓��W�A��)
                m_slipAngle = Mathf.Atan(wheelLocalVelocity.x / wheelLocalVelocity.z);
            }

            // ���C�͂��v�Z

            // �^�C�� 1������̏d��
            var gravity = (m_rigidbody.mass * WEIGHT_RATIO + m_wheelPhysics.wheelMass) * 9.8f;

            // �]�����R(0.015�́C�]�����R�𐄑�����}�W�b�N�i���o�[)
            var rollingResistance = velZDir * gravity * 0.015f;

            // ���C���f���ɂ�門�C�͂̐���
            var baseGrip = m_wheelPhysics.BaseGripCurve.Evaluate(slipAmount);
            var slipRatioGrip = m_wheelPhysics.SlipRatioGripCurve.Evaluate(Mathf.Abs(slipRatio));

            // ���C�W��(�ŏI����)
            m_totalGrip = baseGrip * slipRatioGrip * m_gripFactor;

            // �d�� * ���C�W�� = �^�C���������C�p���[�̍ő�l
            var frictionForce = velZDir * gravity * m_totalGrip;

            // ���C�͂��x�N�g����

            // �O�p�֐��Ŗ��C�͂𕪔z�D
            var targetX = Mathf.Sin(m_slipAngle) * frictionForce;

            var targetZ = rollingResistance;

            if (BrakeInput > 0.1f || achieveMaxRPM)
            {
                targetZ = targetZ + Mathf.Cos(m_slipAngle) * frictionForce;
            }
            else if (m_transmissionConnected)
            {
                targetZ = targetZ + m_torque;
            }

            // �e�͂̍��v���x�N�g���ɕϊ�
            var totalTireForce = m_dummyWheel.transform.TransformDirection(targetX, 0f, targetZ);

            // �T�X�y���V�����ɂ��͂��v�Z

            // �T�X�y���V�����̕ω��ʂ����]���̌W���𑀍삷��
            var springForce = (m_susCps - m_wheelPhysics.susDst * m_wheelPhysics.targetPos) * m_wheelPhysics.spring;
            var damperForce = (m_susCps - m_susCpsPrev) / Time.deltaTime * m_wheelPhysics.damper;
            var suspentionForce = m_raycastHit.normal * (springForce + damperForce);

            // �^�C���ɗ͂�������

            // �T�X�y���V�����ƃ^�C���̃p���[�����ꂼ��ԑ̂̏d�S�ɉ�����
            m_rigidbody.AddForceAtPosition(totalTireForce, m_dummyWheel.position, ForceMode.Force);
            m_rigidbody.AddForceAtPosition(suspentionForce, transform.position, ForceMode.Force);
        }

        private void UpdateEngineRpm()
        {
            // ���݂̃^�C���̏�Ԃ���RPM���ǂꂾ�����������邩����

            if (!m_grounded)
            {
                m_rawWheelRPM = m_currentWheelRpm;
            }

            if (m_driveEnabled)
            {
                if (TransmissionConnected)
                {
                    float increment = 2000f;
                    float decrement = 25f;

                    // �Ԃ��i�s�����ɑ΂��ČX�΂��Ă���Ƃ��̓^�C�����X���b�v���₷������
                    var dst = Mathf.Abs(m_rawWheelRPM * m_totalGearRatio);
                    var angleRatio = m_wheelPhysics.AngleRatioCurve.Evaluate(Mathf.Abs(m_slipAngle / Mathf.PI * 180));
                    var m_torqueRatio = m_wheelPhysics.TorqueRatioCruve.Evaluate(Mathf.Abs(m_torque));
                    var toRawEngineRpm = TLab.Math.LinerApproach(m_engineRpm, GetFrameLerp(increment) * angleRatio * m_torqueRatio, dst);

                    // �G���W���̓��͂��M�A�ɓ`�������Ƃɂ���]���̌���
                    var attenuated = TLab.Math.LinerApproach(toRawEngineRpm, GetFrameLerp(decrement), IDLING - 1);
                    m_feedbackRpm = attenuated;

                    var wheelRpm = Mathf.Sign(m_rawWheelRPM) * attenuated / Mathf.Abs(m_totalGearRatio);
                    m_currentWheelRpm = wheelRpm;
                }
                else
                {
                    float decrement = 0.5f;
                    float targetRpm = 0.0f;
                    m_feedbackRpm = m_engineRpm;
                    m_currentWheelRpm = TLab.Math.LinerApproach(m_rawWheelRPM, GetFrameLerp(decrement), targetRpm);
                }
            }
            else
            {
                float decrement = 0.5f;
                float targetRpm = 0.0f;
                m_currentWheelRpm = TLab.Math.LinerApproach(m_rawWheelRPM, GetFrameLerp(decrement), targetRpm);
            }
        }

        public float DampingWithEngineBrake(float m_engineRPM)
        {
            if (m_transmissionConnected)
            {
                var lerp = TLab.Math.LinerApproach(m_engineRPM, GetFrameLerp(BrakeInput * 50f), 0);
                m_currentWheelRpm = Mathf.Sign(m_rawWheelRPM) * lerp / Mathf.Abs(m_totalGearRatio);
                return lerp > IDLING ? lerp : IDLING - 1;
            }
            else
            {
                m_currentWheelRpm = TLab.Math.LinerApproach(m_currentWheelRpm, GetFrameLerp(BrakeInput * 50f), 0);
                return m_engineRPM;
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
            // �^�C���̃f�o�b�O�p
            m_lineMaterial = new Material(m_lineMaterial);

            m_dummyWheel = new GameObject("DummyWheel").transform;
            m_dummyWheel.transform.position = this.transform.position;
            m_dummyWheel.transform.parent = this.transform.parent;

            m_rigidbody = GetComponentFromParent<Rigidbody>(this.transform, this.GetType().FullName.ToString());

            // raycast��L�������邽�߂ɁC�R���C�_�[��ǉ�
            m_collider = gameObject.AddComponent<SphereCollider>();
            m_collider.center = new Vector3(0.0f, m_wheelPhysics.wheelRadius, 0.0f);
            m_collider.radius = 0.0f;

            m_circleLength = m_wheelPhysics.wheelRadius * 2.0f * Mathf.PI;
        }
    }
}