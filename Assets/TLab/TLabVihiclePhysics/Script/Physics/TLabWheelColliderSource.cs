using UnityEngine;
using static TLab.ComponentUtility;
using static TLab.Math;

public class TLabWheelColliderSource : MonoBehaviour
{
    #region Note

    // rigid body �͕K�� dummy wheel ���������ʒu�ɔz�u���Ȃ���΂Ȃ�Ȃ��D
    // �������Ȃ��ƃ��[���[�����g�����������Ƃ��Arigid body ���n�ʂɉ����t��
    // ����΂��ȃT�X�y���V�����ɂȂ��Ă��܂��D�܂��C����Ƀ��[���[�����g�̐���������Ȃ��D

    // �������͂ɂ���āC�ԑ̂̑��x�x�N�g�����ԑ̂� z���ɑ��������邱�Ƃ��o����΁C
    // �ߓx�ȃ��[�^������������g���N�͂��̕������i�K�ł����邱�ƂɂȂ�C���ʂƂ�
    // �ăO���b�v���s�������o����(�̂�������Ȃ�)�D

    #endregion Note

    [SerializeField] bool steerEnabled = true;
    [SerializeField] bool driveEnabled = false;

    [SerializeField] TLabWheelPhysics wheelPhysics;
    [SerializeField] TLabWheelColliderSource arbPear;

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
    private const float weightRatio = 0.25f;
    private const float diffGearRatio = 3.42f;
    private const float idling = 1400f;
    private const float fixedTime = 30f;

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
            totalGearRatio = value * diffGearRatio;
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
        // layer4(Water)�����𖳎�����

        // ProjectSettings->Physics�� Car�� Water�̏Փ˔������������

        var raycastDistance = wheelPhysics.wheelRadius + wheelPhysics.susDst;

        var ignoreLayer = ~(1 << waterLayer.value);

        var raycastDir = new Ray(dummyWheel.position, -dummyWheel.up);

        if (Physics.Raycast(raycastDir, out raycastHit, raycastDistance, ignoreLayer))
        {
            // Wheel���n�ʂƐڂ��Ă���
            isGrounded = true;
            gizmoColor = Color.green;

            // �O�t���[���̃T�X�y���V�������L���b�V��
            susCpsPrev = susCps;

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
            susCps = 0f;
            gizmoColor = Color.blue;
            isGrounded = false;
        }
    }

    public void UpdateWheel()
    {
        // steerEnabled ���L���̂Ƃ��A�X�e�A�����O��K������D
        var ackermanAngle = steerEnabled ? TLabVihicleInputManager.instance.AckermanAngle : 0;

        //
        // dummy wheel
        //

        Vector3 dummyRot = dummyWheel.localEulerAngles;
        dummyRot.x = 0f;
        dummyRot.y = ackermanAngle;
        dummyRot.z = 0f;
        dummyWheel.localEulerAngles = dummyRot;

        //
        // visual wheel
        //

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
        //
        // ���C���v�Z
        //

        if (isGrounded == false) return;

        // �^�C���̈ړ����x(�^�C���̎��Ƀ��[�J��(m/s))
        var wheelVelocity = rb.GetPointVelocity(dummyWheel.transform.position);
        var wheelLocalVelocity = dummyWheel.transform.InverseTransformDirection(wheelVelocity);

        // �^�C���̉�]�̑��x���Z (m/s)
        var wheelAngularVelocity = currentWheelRPM / 60 * circleLength;

        // �^�C���̑��x�̉�]�����Z (rpm)�D
        rawWheelRPM = wheelLocalVelocity.z / circleLength * 60;

        // �X���b�v�̎����l (m/s)
        var slip_z = wheelAngularVelocity - wheelLocalVelocity.z;
        var slipSqr_z = slip_z * slip_z;
        var slipSqr_x = wheelLocalVelocity.x * wheelLocalVelocity.x;
        var slipAmount = Mathf.Sqrt(slipSqr_x + slipSqr_z);

        // �X���b�v��
        var slipRatio = Mathf.Abs(wheelLocalVelocity.z) > 0.1f ? slip_z / wheelLocalVelocity.z : 2f;

        // �X���b�v�A���O�����t�^���W�F���g�Ōv�Z
        slipAngle = 0f;

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
        var gravity = (rb.mass * weightRatio + wheelPhysics.wheelMass) * 9.8f;

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

        if (TLabVihicleInputManager.instance.BrakeInput > 0.1f)
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

    public float UpdateEngineRPMWithBreak(float tmpRPM)
    {
        //
        // �A�N�Z���ŏ㏸������]�����g���N�̌v�Z�O�Ƀu���[�L�őł������D
        //

        const float maxRPMDecrement = 100f;
        var rpmDecrement = TLabVihicleInputManager.instance.BrakeInput * maxRPMDecrement * Time.fixedDeltaTime;
        if (enableAddTorque)
        {
            // �g�����X�~�b�V�������Ȃ����Ă���̂ŁC�u���[�L���O�ɂ���]���̌����̓A�C�h�����O����ɏI��������D
            var lerpToBrake = LinerApproach(tmpRPM, rpmDecrement, 0);
            var currentTotalGear = Mathf.Abs(gearRatio) * diffGearRatio;
            currentWheelRPM = lerpToBrake / currentTotalGear;
            return lerpToBrake > idling ? lerpToBrake : idling - 1;
        }
        else
        {
            // �g�����X�~�b�V�������O��Ă���̂ŁC�^�C���̉�]�X�P�[���Ōv�Z
            currentWheelRPM = LinerApproach(currentWheelRPM, rpmDecrement, 0);
            return tmpRPM;
        }
    }

    private void UpdateEngineRPMWithEngineAxis()
    {
        //
        // �^�C���̉�]���̍X�V.1 (�G���W���X�P�[��)
        //

        var dst = Mathf.Abs(rawWheelRPM * totalGearRatio);

        const float rpmForGripMax = 2000f * fixedTime;
        // �Ԃ��i�s�����ɑ΂��ČX�΂��Ă���Ƃ��̓^�C�����X���b�v���₷������
        var angleRatio = wheelPhysics.AngleRatioCurve.Evaluate(Mathf.Abs(slipAngle / Mathf.PI * 180));
        var torqueRatio = wheelPhysics.TorqueRatioCruve.Evaluate(Mathf.Abs(torque));

        var rpmForGrip = rpmForGripMax * angleRatio * torqueRatio * Time.fixedDeltaTime;
        var driftRPM = engineRpm;
        var toRawEngineRpm = LinerApproach(driftRPM, rpmForGrip, dst);

        // �G���W���̓��͂��M�A�ɓ`�������Ƃɂ���]���̌���
        const float rpmAttenuation = 25f * fixedTime;
        var lerpToAttenuation = LinerApproach(toRawEngineRpm, rpmAttenuation * Time.fixedDeltaTime, idling - 1);
        feedbackRpm = lerpToAttenuation;

        //
        // �^�C���̉�]���̍X�V.2
        //

        var wheelRpm = Mathf.Sign(rawWheelRPM) * lerpToAttenuation / Mathf.Abs(totalGearRatio);
        currentWheelRPM = wheelRpm;
    }

    public void UpdateEngineRpm()
    {
        const float decrement = 15f;

        if (isGrounded)
        {
            // �^�C�����n�ʂ�͂�ł���

            if (driveEnabled)
            {
                // �쓮��

                if (enableAddTorque || rawWheelRPM * totalGearRatio >= idling && TLabVihicleInputManager.instance.ClutchInput <= 0.5f)
                {
                    // �M�A���Ȃ����Ă���
                    UpdateEngineRPMWithEngineAxis();
                }
                else
                {
                    feedbackRpm = engineRpm;
                    currentWheelRPM = LinerApproach(rawWheelRPM, decrement * Time.fixedDeltaTime, 0);
                }
            }
            else
            {
                currentWheelRPM = LinerApproach(rawWheelRPM, decrement * Time.fixedDeltaTime, 0);
            }
        }
        else
        {
            // �^�C�����n�ʂ�͂�ł��Ȃ�

            const float rpmAttenuation = 15f;
            rawWheelRPM = LinerApproach(currentWheelRPM, rpmAttenuation * Time.fixedDeltaTime, 0);
            if (driveEnabled)
            {
                if (enableAddTorque || rawWheelRPM * totalGearRatio >= idling && TLabVihicleInputManager.instance.ClutchInput <= 0.5f)
                {
                    UpdateEngineRPMWithEngineAxis();
                }
                else
                {
                    feedbackRpm = engineRpm;
                    currentWheelRPM = LinerApproach(rawWheelRPM, decrement * Time.fixedDeltaTime, 0);
                }
            }
            else
            {
                currentWheelRPM = LinerApproach(rawWheelRPM, decrement * Time.fixedDeltaTime, 0);
            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
        }
    }

    public void TLabStart()
    {
        dummyWheel = new GameObject("DummyWheel").transform;
        dummyWheel.transform.position = this.transform.position;
        dummyWheel.transform.parent = this.transform.parent;

        rb = GetComponentFromParent<Rigidbody>(this.transform,this.GetType().FullName.ToString());

        // Add appropriate Collider to enable RaycastHit
        m_collider = gameObject.AddComponent<SphereCollider>();
        m_collider.center = new Vector3(0, wheelPhysics.wheelRadius, 0);
        m_collider.radius = 0f;

        circleLength = wheelPhysics.wheelRadius * 2 * Mathf.PI;

        waterLayer = LayerMask.NameToLayer("Water");
    }
}
