using UnityEngine;
using static TLab.ComponentUtility;
using static TLab.Math;

namespace TLab.VehiclePhysics
{
    public class WheelColliderSource : MonoBehaviour
    {
        #region Note
        // rigid body は必ず dummy wheel よりも高い位置に配置しなければならない．
        // そうしないとヨーモーメントが発生したとき、rigid body が地面に押し付け
        // られる可笑しなサスペンションになってしまう．また，それにヨーモーメントの制御も効かない．

        // 強い横力によって，車体の速度ベクトルを車体の z軸に早く向けることが出来れば，
        // 過度なヨー運動を減衰するトルクはその分早い段階でかかることになり，結果とし
        // てグリップ走行を実現できそう．
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
            // Layer (Water)を無視
            // ProjectSettings->Physicsで Carと Waterの衝突判定を解除する

            var dir = new Ray(dummyWheel.position, -dummyWheel.up);
            var ignore = ~(1 << waterLayer.value);
            var distance = wheelPhysics.wheelRadius + wheelPhysics.susDst;

            if (Physics.Raycast(dir, out raycastHit, distance, ignore))
            {
                // Wheelが地面と接している
                isGrounded = true;
                susCpsPrev = susCps;
                gizmoColor = Color.green;

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

            // タイヤの移動速度(タイヤの軸にローカル(m/s))
            var wheelVelocity = rb.GetPointVelocity(dummyWheel.transform.position);
            var wheelLocalVelocity = dummyWheel.transform.InverseTransformDirection(wheelVelocity);

            // タイヤの回転の速度換算 (m/s)
            var wheelAngularVelocity = currentWheelRPM * WheelRPM2Vel;

            // タイヤの速度の回転数換算 (rpm)．
            // vel2WheelRPMが MaxWheelRPMを上回る時は rawWheelRPMをMaxWheelRPMに制限してエンジンブレーキを発生させる．
            var vel2WheelRPM = wheelLocalVelocity.z * Vel2WheelRPM;
            var achieveMaxRPM = Mathf.Abs(vel2WheelRPM) >= MaxWheelRPM;
            rawWheelRPM = achieveMaxRPM ? Mathf.Sign(vel2WheelRPM) * MaxWheelRPM : vel2WheelRPM;
            //Debug.Log(achieveMaxRPM + " " + Mathf.Abs(vel2WheelRPM) + " " + MaxWheelRPM);

            // スリップの実測値 (m/s)
            var slipZ = wheelAngularVelocity - (rawWheelRPM * WheelRPM2Vel);
            var slipSqrZ = slipZ * slipZ;
            var slipSqrX = wheelLocalVelocity.x * wheelLocalVelocity.x;
            var slipAmount = Mathf.Sqrt(slipSqrX + slipSqrZ);
            //Debug.Log(slipZ + " " + wheelAngularVelocity + " " + (rawWheelRPM * WheelRPM2Vel) + " " + this.gameObject.name);

            // スリップ率
            var slipRatio = Mathf.Abs(wheelLocalVelocity.z) > 0.1f ? slipZ / wheelLocalVelocity.z : 2f;

            // スリップアングルを逆タンジェントで計算
            slipAngle = 0.0f;

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
            var gravity = (rb.mass * WEIGHT_RATIO + wheelPhysics.wheelMass) * 9.8f;

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

            if (BrakeInput > 0.1f || achieveMaxRPM)
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

        public float UpdateEngineRPMWithBreak(float engineRPM)
        {
            //
            // アクセルで上昇した回転数をトルクの計算前にブレーキで打ち消す．
            //

            if (enableAddTorque)
            {
                // トランスミッションがつながっている
                var lerp = LinerApproach(engineRPM, GetFrameLerp(BrakeInput * 50f), 0);
                currentWheelRPM = Mathf.Sign(rawWheelRPM) * lerp / Mathf.Abs(totalGearRatio);
                return lerp > IDLING ? lerp : IDLING - 1;
            }
            else
            {
                // トランスミッションが外れているので，タイヤの回転スケールで計算
                currentWheelRPM = LinerApproach(currentWheelRPM, GetFrameLerp(BrakeInput * 50f), 0);
                return engineRPM;
            }
        }

        private void UpdateEngineRPMWithEngineAxis()
        {
            var dst = Mathf.Abs(rawWheelRPM * totalGearRatio);

            // 現在のタイヤの状態からRPMをどれだけ減衰させるか決定
            // 車が進行方向に対して傾斜しているときはタイヤがスリップしやすくする
            var angleRatio = wheelPhysics.AngleRatioCurve.Evaluate(Mathf.Abs(slipAngle / Mathf.PI * 180));
            var torqueRatio = wheelPhysics.TorqueRatioCruve.Evaluate(Mathf.Abs(torque));
            var toRawEngineRpm = LinerApproach(engineRpm, GetFrameLerp(2000f) * angleRatio * torqueRatio, dst);

            // エンジンの動力をギアに伝えたことによる回転数の減衰
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
                    // ギアがつながっている
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