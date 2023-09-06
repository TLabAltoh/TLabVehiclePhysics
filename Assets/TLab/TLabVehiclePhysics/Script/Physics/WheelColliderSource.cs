using UnityEngine;
using static TLab.ComponentUtility;
using static TLab.Math;

namespace TLab.VehiclePhysics
{
    public class WheelColliderSource : MonoBehaviour
    {
        #region Note
        // rigid body �͕K�� dummy wheel ���������ʒu�ɔz�u���Ȃ���΂Ȃ�Ȃ��D
        // �������Ȃ��ƃ��[���[�����g�����������Ƃ��Arigid body ���n�ʂɉ����t��
        // ����΂��ȃT�X�y���V�����ɂȂ��Ă��܂��D�܂��C����Ƀ��[���[�����g�̐���������Ȃ��D

        // �������͂ɂ���āC�ԑ̂̑��x�x�N�g�����ԑ̂� z���ɑ��������邱�Ƃ��o����΁C
        // �ߓx�ȃ��[�^������������g���N�͂��̕������i�K�ł����邱�ƂɂȂ�C���ʂƂ�
        // �ăO���b�v���s�������ł������D
        #endregion Note

        [SerializeField] bool steerEnabled = true;
        [SerializeField] bool driveEnabled = false;

        [SerializeField] VehicleEngine engine;
        [SerializeField] VehicleInputManager inputManager;
        [SerializeField] WheelPhysics wheelPhysics;
        [SerializeField] WheelColliderSource arbPear;

        private SphereCollider m_collider;
        private Transform dummyWheel;
        private Rigidbody rb;
        private RaycastHit raycastHit;
        private Color gizmoColor;
        private LayerMask waterLayer;
        private float wheelRot = 0f;
        private float rawWheelRPM = 0f;
        private float currentWheelRPM = 0f;
        private float slipAngle = 0f;
        private float gripFactor = 1f;
        private float totalGrip = 1f;
        private float engineRpm = 0f;
        private float feedbackRpm = 0f;
        private float torque = 0f;
        private float gearRatio = 0f;
        private float totalGearRatio = 0f;
        private float circleLength;
        private float susCps = 0f;
        private float susCpsPrev = 0f;
        private float compressRate = 0f;
        private bool enableAddTorque = false;
        private bool isGrounded = false;
        private const float WEIGHT_RATIO = 0.25f;
        private const float DIFFGEAR_RATIO = 3.42f;
        private const float IDLING = 1400f;
        private const float FIXED_TIME = 30f;

        #region Passing data to and from the engine
        public float EngineRpm
        {
            get
            {
                return engineRpm;
            }

            set
            {
                engineRpm = value;
            }
        }

        public float FeedbackRpm
        {
            get
            {
                return feedbackRpm;
            }

            set
            {
                feedbackRpm = value;
            }
        }

        public float Torque
        {
            get
            {
                return torque;
            }

            set
            {
                torque = value * totalGearRatio / wheelPhysics.wheelRadius;
            }
        }

        public bool EnableAddTorque
        {
            get
            {
                return enableAddTorque;
            }

            set
            {
                enableAddTorque = value;
            }
        }

        public float GearRatio
        {
            get
            {
                return gearRatio;
            }

            set
            {
                gearRatio = value;
                totalGearRatio = value * DIFFGEAR_RATIO;
            }
        }

        public float TotalGrip
        {
            get
            {
                return totalGrip;
            }
        }

        public float GripFactor
        {
            set
            {
                gripFactor = value;
            }
        }
        #endregion Passing data to and from the engine

        #region Input
        private float ActualInput
        {
            get
            {
                return inputManager.ActualInput;
            }
        }

        private float BrakeInput
        {
            get
            {
                return inputManager.BrakeInput;
            }
        }

        private float AckermanAngle
        {
            get
            {
                return inputManager.AckermanAngle;
            }
        }

        private float ClutchInput
        {
            get
            {
                return inputManager.ClutchInput;
            }
        }
        #endregion Input

        private bool GearConnected
        {
            get
            {
                return enableAddTorque || rawWheelRPM * totalGearRatio >= IDLING && ClutchInput <= 0.5f;
            }
        }

        private float MaxWheelRPM
        {
            get
            {
                if (totalGearRatio != 0)
                    return engine.EngineMaxRpm / Mathf.Abs(totalGearRatio);
                else
                    return Mathf.Infinity;
            }
        }

        private float WheelRPM2Vel
        {
            get
            {
                return circleLength / 60;
            }
        }

        private float Vel2WheelRPM
        {
            get
            {
                return 60 / circleLength;
            }
        }

        private float GetFrameLerp(float value)
        {
            return value * FIXED_TIME * Time.fixedDeltaTime;
        }

        //public void OndrawGizmosSelected()
        //{
        //    Gizmos.color = gizmoColor;

        //    Gizmos.DrawLine(

        //        // wheel�̉��[�܂�
        //        transform.position - dummyWheel.up * wheelPhysics.wheelRadius,

        //        // �^�C���̒��S���猻�݂̃T�X�y���V�����̐L�т܂�
        //        transform.position + (dummyWheel.up * (wheelPhysics.susDst - susCps))
        //    );

        //    Vector3 startPoint = new Vector3();
        //    startPoint.x = 0f;
        //    startPoint.y = Mathf.Sin(0) * wheelPhysics.wheelRadius;
        //    startPoint.z = Mathf.Cos(0) * wheelPhysics.wheelRadius;

        //    // transformpoint�Ń��[�J����Ԃ��烏�[���h��Ԃ֕ϊ�
        //    Vector3 point0 = transform.TransformPoint(startPoint);

        //    for (int i = 1; i < 20; ++i)
        //    {
        //        float tempsin = Mathf.Sin(i / 20f * Mathf.PI * 2);
        //        float tempcos = Mathf.Cos(i / 20f * Mathf.PI * 2);
        //        Vector3 susvec = new Vector3(0, tempsin, tempcos);
        //        Vector3 point1 = transform.TransformPoint(wheelPhysics.wheelRadius * susvec);
        //        Gizmos.DrawLine(point0, point1);
        //        point0 = point1;
        //    }

        //    Gizmos.color = Color.white;
        //}

        public void UpdateSuspension()
        {
            // Layer (Water)�𖳎�
            // ProjectSettings->Physics�� Car�� Water�̏Փ˔������������

            var dir = new Ray(dummyWheel.position, -dummyWheel.up);
            var ignore = ~(1 << waterLayer.value);
            var distance = wheelPhysics.wheelRadius + wheelPhysics.susDst;

            if (Physics.Raycast(dir, out raycastHit, distance, ignore))
            {
                // Wheel���n�ʂƐڂ��Ă���
                isGrounded = true;
                susCpsPrev = susCps;
                gizmoColor = Color.green;

                var stretchedOut = wheelPhysics.wheelRadius + wheelPhysics.susDst;
                var suspentionOrigin = dummyWheel.position;
                susCps = stretchedOut - (raycastHit.point - suspentionOrigin).magnitude;

                // �T�X�y���V�����̈��k��(���k���Ă���قǒl��1�ɋ߂Â�)
                compressRate = susCps / wheelPhysics.susDst;

                if (susCps > wheelPhysics.susDst)
                {
                    // �T�X�y���V�������Ԃ�Ă���
                    gizmoColor = Color.red;
                    compressRate = 1f;
                }
            }
            else
            {
                // Wheel���n�ʂƐڂ��Ă��Ȃ��Ƃ�
                isGrounded = false;
                susCps = 0f;
                gizmoColor = Color.blue;
            }
        }

        public void UpdateWheel()
        {
            var ackermanAngle = steerEnabled ? AckermanAngle : 0;

            // dummy wheel
            Vector3 dummyRot = dummyWheel.localEulerAngles;
            dummyRot.x = 0f;
            dummyRot.y = ackermanAngle;
            dummyRot.z = 0f;
            dummyWheel.localEulerAngles = dummyRot;

            // visual wheel
            wheelRot += currentWheelRPM / 60 * 360 * Time.fixedDeltaTime;
            Vector3 visualRot = transform.localEulerAngles;
            visualRot.x = wheelRot;
            visualRot.y = ackermanAngle;
            visualRot.z = 0f;
            transform.localEulerAngles = visualRot;

            var suspentionOrigin = dummyWheel.localPosition;
            var amountOfReduction = Vector3.up * (wheelPhysics.susDst - susCps);
            transform.localPosition = suspentionOrigin - amountOfReduction;
        }

        public void WheelAddForce()
        {
            if (isGrounded == false) return;

            // �^�C���̈ړ����x(�^�C���̎��Ƀ��[�J��(m/s))
            var wheelVelocity = rb.GetPointVelocity(dummyWheel.transform.position);
            var wheelLocalVelocity = dummyWheel.transform.InverseTransformDirection(wheelVelocity);

            // �^�C���̉�]�̑��x���Z (m/s)
            var wheelAngularVelocity = currentWheelRPM * WheelRPM2Vel;

            // �^�C���̑��x�̉�]�����Z (rpm)�D
            // vel2WheelRPM�� MaxWheelRPM�����鎞�� rawWheelRPM��MaxWheelRPM�ɐ������ăG���W���u���[�L�𔭐�������D
            var vel2WheelRPM = wheelLocalVelocity.z * Vel2WheelRPM;
            var achieveMaxRPM = Mathf.Abs(vel2WheelRPM) >= MaxWheelRPM;
            rawWheelRPM = achieveMaxRPM ? Mathf.Sign(vel2WheelRPM) * MaxWheelRPM : vel2WheelRPM;
            //Debug.Log(achieveMaxRPM + " " + Mathf.Abs(vel2WheelRPM) + " " + MaxWheelRPM);

            // �X���b�v�̎����l (m/s)
            var slipZ = wheelAngularVelocity - (rawWheelRPM * WheelRPM2Vel);
            var slipSqrZ = slipZ * slipZ;
            var slipSqrX = wheelLocalVelocity.x * wheelLocalVelocity.x;
            var slipAmount = Mathf.Sqrt(slipSqrX + slipSqrZ);
            //Debug.Log(slipZ + " " + wheelAngularVelocity + " " + (rawWheelRPM * WheelRPM2Vel) + " " + this.gameObject.name);

            // �X���b�v��
            var slipRatio = Mathf.Abs(wheelLocalVelocity.z) > 0.1f ? slipZ / wheelLocalVelocity.z : 2f;

            // �X���b�v�A���O�����t�^���W�F���g�Ōv�Z
            slipAngle = 0.0f;

            // ���x�̌���
            var velZDir = -System.Math.Sign(wheelLocalVelocity.z);

            // 0���Z�G���[��h�~
            if (velZDir == 0)
            {
                // wheelLocalVelocity.z = 0 : ���łɐ���
                // wheelLocalVelocity.x = 0 : ���ꂪ���������� --> slipAngle = 0
                slipAngle = System.Math.Sign(wheelLocalVelocity.x) * 90 * Mathf.Deg2Rad;
            }
            else
            {
                // �t�^���W�F���g�ŃX���b�v�A���O�����v�Z(�Ԃ�l�̓��W�A��)
                slipAngle = Mathf.Atan(wheelLocalVelocity.x / wheelLocalVelocity.z);
            }

            //
            // ���C�͂��v�Z
            //

            // �^�C�� 1������̏d��
            var gravity = (rb.mass * WEIGHT_RATIO + wheelPhysics.wheelMass) * 9.8f;

            // �]�����R(0.015�́C�]�����R�𐄑�����}�W�b�N�i���o�[)
            var rollingResistance = velZDir * gravity * 0.015f;

            // ���C���f���ɂ�門�C�͂̐���
            var baseGrip = wheelPhysics.BaseGripCurve.Evaluate(slipAmount);
            var slipRatioGrip = wheelPhysics.SlipRatioGripCurve.Evaluate(Mathf.Abs(slipRatio));

            // ���C�W��(�ŏI����)
            totalGrip = baseGrip * slipRatioGrip * gripFactor;

            // �d�� * ���C�W�� = �^�C���������C�p���[�̍ő�l
            var frictionForce = velZDir * gravity * totalGrip;

            //
            // ���C�͂��x�N�g����
            //

            // �O�p�֐��Ŗ��C�͂𕪔z�D
            var targetX = Mathf.Sin(slipAngle) * frictionForce;

            var targetZ = rollingResistance;

            if (BrakeInput > 0.1f || achieveMaxRPM)
                targetZ = targetZ + Mathf.Cos(slipAngle) * frictionForce;
            else if (enableAddTorque)
                targetZ = targetZ + torque;

            // �e�͂̍��v���x�N�g���ɕϊ�
            var totalTireForce = dummyWheel.transform.TransformDirection(targetX, 0f, targetZ);

            //
            // �T�X�y���V�����ɂ��͂��v�Z
            //

            // �T�X�y���V�����̕ω��ʂ����]���̌W���𑀍삷��
            var springForce = (susCps - wheelPhysics.susDst * wheelPhysics.targetPos) * wheelPhysics.spring;
            var damperForce = (susCps - susCpsPrev) / Time.deltaTime * wheelPhysics.damper;
            var suspentionForce = raycastHit.normal * (springForce + damperForce);

            //
            // �^�C���ɗ͂�������
            //

            // �T�X�y���V�����ƃ^�C���̃p���[�����ꂼ��ԑ̂̏d�S�ɉ�����
            rb.AddForceAtPosition(totalTireForce, dummyWheel.position, ForceMode.Force);
            rb.AddForceAtPosition(suspentionForce, transform.position, ForceMode.Force);
        }

        public float UpdateEngineRPMWithBreak(float engineRPM)
        {
            //
            // �A�N�Z���ŏ㏸������]�����g���N�̌v�Z�O�Ƀu���[�L�őł������D
            //

            if (enableAddTorque)
            {
                // �g�����X�~�b�V�������Ȃ����Ă���
                var lerp = LinerApproach(engineRPM, GetFrameLerp(BrakeInput * 50f), 0);
                currentWheelRPM = Mathf.Sign(rawWheelRPM) * lerp / Mathf.Abs(totalGearRatio);
                return lerp > IDLING ? lerp : IDLING - 1;
            }
            else
            {
                // �g�����X�~�b�V�������O��Ă���̂ŁC�^�C���̉�]�X�P�[���Ōv�Z
                currentWheelRPM = LinerApproach(currentWheelRPM, GetFrameLerp(BrakeInput * 50f), 0);
                return engineRPM;
            }
        }

        private void UpdateEngineRPMWithEngineAxis()
        {
            var dst = Mathf.Abs(rawWheelRPM * totalGearRatio);

            // ���݂̃^�C���̏�Ԃ���RPM���ǂꂾ�����������邩����
            // �Ԃ��i�s�����ɑ΂��ČX�΂��Ă���Ƃ��̓^�C�����X���b�v���₷������
            var angleRatio = wheelPhysics.AngleRatioCurve.Evaluate(Mathf.Abs(slipAngle / Mathf.PI * 180));
            var torqueRatio = wheelPhysics.TorqueRatioCruve.Evaluate(Mathf.Abs(torque));
            var toRawEngineRpm = LinerApproach(engineRpm, GetFrameLerp(2000f) * angleRatio * torqueRatio, dst);

            // �G���W���̓��͂��M�A�ɓ`�������Ƃɂ���]���̌���
            var attenuated = LinerApproach(toRawEngineRpm, GetFrameLerp(25f), IDLING - 1);
            feedbackRpm = attenuated;

            var wheelRpm = Mathf.Sign(rawWheelRPM) * attenuated / Mathf.Abs(totalGearRatio);
            currentWheelRPM = wheelRpm;
        }

        public void UpdateEngineRpm()
        {
            if (!isGrounded) rawWheelRPM = currentWheelRPM;

            if (driveEnabled)
            {
                if (GearConnected)
                {
                    // �M�A���Ȃ����Ă���
                    UpdateEngineRPMWithEngineAxis();
                }
                else
                {
                    feedbackRpm = engineRpm;
                    currentWheelRPM = LinerApproach(rawWheelRPM, GetFrameLerp(0.5f), 0);
                }
            }
            else
                currentWheelRPM = LinerApproach(rawWheelRPM, GetFrameLerp(0.5f), 0);
        }

        public void TLabStart()
        {
            dummyWheel = new GameObject("DummyWheel").transform;
            dummyWheel.transform.position = this.transform.position;
            dummyWheel.transform.parent = this.transform.parent;

            rb = GetComponentFromParent<Rigidbody>(this.transform, this.GetType().FullName.ToString());

            // Add appropriate Collider to enable RaycastHit
            m_collider = gameObject.AddComponent<SphereCollider>();
            m_collider.center = new Vector3(0, wheelPhysics.wheelRadius, 0);
            m_collider.radius = 0f;

            circleLength = wheelPhysics.wheelRadius * 2 * Mathf.PI;

            waterLayer = LayerMask.NameToLayer("Water");
        }
    }
}