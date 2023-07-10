using UnityEngine;
using static TLab.ComponentUtility;
using static TLab.Math;

public class TLabWheelColliderSource : MonoBehaviour
{
    #region Note

    // rigid body は必ず dummy wheel よりも高い位置に配置しなければならない．
    // そうしないとヨーモーメントが発生したとき、rigid body が地面に押し付け
    // られる可笑しなサスペンションになってしまう．また，それにヨーモーメントの制御も効かない．

    // 強い横力によって，車体の速度ベクトルを車体の z軸に早く向けることが出来れば，
    // 過度なヨー運動を減衰するトルクはその分早い段階でかかることになり，結果とし
    // てグリップ走行を実現出来る(のかもしれない)．

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

    //        // wheelの下端まで
    //        transform.position - dummyWheel.up * wheelPhysics.wheelRadius,

    //        // タイヤの中心から現在のサスペンションの伸びまで
    //        transform.position + (dummyWheel.up * (wheelPhysics.susDst - susCps))
    //    );

    //    Vector3 startPoint = new Vector3();
    //    startPoint.x = 0f;
    //    startPoint.y = Mathf.Sin(0) * wheelPhysics.wheelRadius;
    //    startPoint.z = Mathf.Cos(0) * wheelPhysics.wheelRadius;

    //    // transformpointでローカル空間からワールド空間へ変換
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
        // layer4(Water)だけを無視する

        // ProjectSettings->Physicsで Carと Waterの衝突判定を解除する

        var raycastDistance = wheelPhysics.wheelRadius + wheelPhysics.susDst;

        var ignoreLayer = ~(1 << waterLayer.value);

        var raycastDir = new Ray(dummyWheel.position, -dummyWheel.up);

        if (Physics.Raycast(raycastDir, out raycastHit, raycastDistance, ignoreLayer))
        {
            // Wheelが地面と接している
            isGrounded = true;
            gizmoColor = Color.green;

            // 前フレームのサスペンションをキャッシュ
            susCpsPrev = susCps;

            var stretchedOut = wheelPhysics.wheelRadius + wheelPhysics.susDst;
            var suspentionOrigin = dummyWheel.position;

            susCps = stretchedOut - (raycastHit.point - suspentionOrigin).magnitude;

            // サスペンションの圧縮率(圧縮しているほど値が1に近づく)
            compressRate = susCps / wheelPhysics.susDst;

            if (susCps > wheelPhysics.susDst)
            {
                // サスペンションがつぶれている
                gizmoColor = Color.red;
                compressRate = 1f;
            }
        }
        else
        {
            // Wheelが地面と接していないとき
            susCps = 0f;
            gizmoColor = Color.blue;
            isGrounded = false;
        }
    }

    public void UpdateWheel()
    {
        // steerEnabled が有効のとき、ステアリングを適応する．
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
        // 摩擦を計算
        //

        if (isGrounded == false) return;

        // タイヤの移動速度(タイヤの軸にローカル(m/s))
        var wheelVelocity = rb.GetPointVelocity(dummyWheel.transform.position);
        var wheelLocalVelocity = dummyWheel.transform.InverseTransformDirection(wheelVelocity);

        // タイヤの回転の速度換算 (m/s)
        var wheelAngularVelocity = currentWheelRPM / 60 * circleLength;

        // タイヤの速度の回転数換算 (rpm)．
        rawWheelRPM = wheelLocalVelocity.z / circleLength * 60;

        // スリップの実測値 (m/s)
        var slip_z = wheelAngularVelocity - wheelLocalVelocity.z;
        var slipSqr_z = slip_z * slip_z;
        var slipSqr_x = wheelLocalVelocity.x * wheelLocalVelocity.x;
        var slipAmount = Mathf.Sqrt(slipSqr_x + slipSqr_z);

        // スリップ率
        var slipRatio = Mathf.Abs(wheelLocalVelocity.z) > 0.1f ? slip_z / wheelLocalVelocity.z : 2f;

        // スリップアングルを逆タンジェントで計算
        slipAngle = 0f;

        // 速度の向き
        var velZDir = -System.Math.Sign(wheelLocalVelocity.z);

        // 0除算エラーを防止
        if (velZDir == 0)
        {
            // wheelLocalVelocity.z = 0 : すでに成立
            // wheelLocalVelocity.x = 0 : これが成立したら --> slipAngle = 0
            slipAngle = System.Math.Sign(wheelLocalVelocity.x) * 90 * Mathf.Deg2Rad;
        }
        else
        {
            // 逆タンジェントでスリップアングルを計算(返り値はラジアン)
            slipAngle = Mathf.Atan(wheelLocalVelocity.x / wheelLocalVelocity.z);
        }

        //
        // 摩擦力を計算
        //

        // タイヤ 1つ当たりの重力
        var gravity = (rb.mass * weightRatio + wheelPhysics.wheelMass) * 9.8f;

        // 転がり抵抗(0.015は，転がり抵抗を推測するマジックナンバー)
        var rollingResistance = velZDir * gravity * 0.015f;

        // 摩擦モデルによる摩擦力の推測
        var baseGrip = wheelPhysics.BaseGripCurve.Evaluate(slipAmount);
        var slipRatioGrip = wheelPhysics.SlipRatioGripCurve.Evaluate(Mathf.Abs(slipRatio));

        // 摩擦係数(最終決定)
        totalGrip = baseGrip * slipRatioGrip * gripFactor;

        // 重力 * 摩擦係数 = タイヤが持つ摩擦パワーの最大値
        var frictionForce = velZDir * gravity * totalGrip;

        //
        // 摩擦力をベクトル化
        //

        // 三角関数で摩擦力を分配．
        var targetX = Mathf.Sin(slipAngle) * frictionForce;

        var targetZ = rollingResistance;

        if (TLabVihicleInputManager.instance.BrakeInput > 0.1f)
            targetZ = targetZ + Mathf.Cos(slipAngle) * frictionForce;
        else if (enableAddTorque)
            targetZ = targetZ + torque;

        // 各力の合計をベクトルに変換
        var totalTireForce = dummyWheel.transform.TransformDirection(targetX, 0f, targetZ);

        //
        // サスペンションによる力を計算
        //

        // サスペンションの変化量から回転数の係数を操作する
        var springForce = (susCps - wheelPhysics.susDst * wheelPhysics.targetPos) * wheelPhysics.spring;
        var damperForce = (susCps - susCpsPrev) / Time.deltaTime * wheelPhysics.damper;
        var suspentionForce = raycastHit.normal * (springForce + damperForce);

        //
        // タイヤに力を加える
        //

        // サスペンションとタイヤのパワーをそれぞれ車体の重心に加える
        rb.AddForceAtPosition(totalTireForce, dummyWheel.position, ForceMode.Force);
        rb.AddForceAtPosition(suspentionForce, transform.position, ForceMode.Force);
    }

    public float UpdateEngineRPMWithBreak(float tmpRPM)
    {
        //
        // アクセルで上昇した回転数をトルクの計算前にブレーキで打ち消す．
        //

        const float maxRPMDecrement = 100f;
        var rpmDecrement = TLabVihicleInputManager.instance.BrakeInput * maxRPMDecrement * Time.fixedDeltaTime;
        if (enableAddTorque)
        {
            // トランスミッションがつながっているので，ブレーキングによる回転数の減衰はアイドリングを基準に終了させる．
            var lerpToBrake = LinerApproach(tmpRPM, rpmDecrement, 0);
            var currentTotalGear = Mathf.Abs(gearRatio) * diffGearRatio;
            currentWheelRPM = lerpToBrake / currentTotalGear;
            return lerpToBrake > idling ? lerpToBrake : idling - 1;
        }
        else
        {
            // トランスミッションが外れているので，タイヤの回転スケールで計算
            currentWheelRPM = LinerApproach(currentWheelRPM, rpmDecrement, 0);
            return tmpRPM;
        }
    }

    private void UpdateEngineRPMWithEngineAxis()
    {
        //
        // タイヤの回転数の更新.1 (エンジンスケール)
        //

        var dst = Mathf.Abs(rawWheelRPM * totalGearRatio);

        const float rpmForGripMax = 2000f * fixedTime;
        // 車が進行方向に対して傾斜しているときはタイヤがスリップしやすくする
        var angleRatio = wheelPhysics.AngleRatioCurve.Evaluate(Mathf.Abs(slipAngle / Mathf.PI * 180));
        var torqueRatio = wheelPhysics.TorqueRatioCruve.Evaluate(Mathf.Abs(torque));

        var rpmForGrip = rpmForGripMax * angleRatio * torqueRatio * Time.fixedDeltaTime;
        var driftRPM = engineRpm;
        var toRawEngineRpm = LinerApproach(driftRPM, rpmForGrip, dst);

        // エンジンの動力をギアに伝えたことによる回転数の減衰
        const float rpmAttenuation = 25f * fixedTime;
        var lerpToAttenuation = LinerApproach(toRawEngineRpm, rpmAttenuation * Time.fixedDeltaTime, idling - 1);
        feedbackRpm = lerpToAttenuation;

        //
        // タイヤの回転数の更新.2
        //

        var wheelRpm = Mathf.Sign(rawWheelRPM) * lerpToAttenuation / Mathf.Abs(totalGearRatio);
        currentWheelRPM = wheelRpm;
    }

    public void UpdateEngineRpm()
    {
        const float decrement = 15f;

        if (isGrounded)
        {
            // タイヤが地面を掴んでいる

            if (driveEnabled)
            {
                // 駆動輪

                if (enableAddTorque || rawWheelRPM * totalGearRatio >= idling && TLabVihicleInputManager.instance.ClutchInput <= 0.5f)
                {
                    // ギアがつながっている
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
            // タイヤが地面を掴んでいない

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
