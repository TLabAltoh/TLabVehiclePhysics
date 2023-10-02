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

            // タイヤのタイヤ軸にローカルな移動速度 (m/s)
            var wheelVelocity = m_rigidbody.GetPointVelocity(m_dummyWheel.transform.position);
            var wheelLocalVelocity = m_dummyWheel.transform.InverseTransformDirection(wheelVelocity);

            // タイヤの回転の速度換算 (m/s)
            var wheelAngularVelocity = m_currentWheelRpm * WheelRPM2Vel;

            // タイヤの速度の回転数換算 (rpm)
            var vel2WheelRPM = wheelLocalVelocity.z * Vel2WheelRPM;
            var achieveMaxRPM = Mathf.Abs(vel2WheelRPM) >= MaxWheelRPM;

            // vel2WheelRPMが MaxWheelRPMを上回る時は m_rawWheelRPMをMaxWheelRPMに制限してエンジンブレーキを発生させる
            m_rawWheelRPM = achieveMaxRPM ? Mathf.Sign(vel2WheelRPM) * MaxWheelRPM : vel2WheelRPM;

            // スリップの実測値 (m/s)
            var slipZ = wheelAngularVelocity - (m_rawWheelRPM * WheelRPM2Vel);
            var slipSqrZ = slipZ * slipZ;
            var slipSqrX = wheelLocalVelocity.x * wheelLocalVelocity.x;
            var slipAmount = Mathf.Sqrt(slipSqrX + slipSqrZ);

            // スリップ率
            var slipRatio = Mathf.Abs(wheelLocalVelocity.z) > 0.1f ? slipZ / wheelLocalVelocity.z : 2f;

            // スリップアングルを逆タンジェントで計算
            m_slipAngle = 0.0f;

            // 速度の向き
            var velZDir = -System.Math.Sign(wheelLocalVelocity.z);

            // 0除算エラーを防止
            if (velZDir == 0)
            {
                // wheelLocalVelocity.z = 0 : すでに成立
                // wheelLocalVelocity.x = 0 : これも成立したら --> m_slipAngle = 0
                m_slipAngle = System.Math.Sign(wheelLocalVelocity.x) * 90 * Mathf.Deg2Rad;
            }
            else
            {
                // 逆タンジェントでスリップアングルを計算(返り値はラジアン)
                m_slipAngle = Mathf.Atan(wheelLocalVelocity.x / wheelLocalVelocity.z);
            }

            // 摩擦力を計算

            // タイヤ 1つ当たりの重力
            var gravity = (m_rigidbody.mass * WEIGHT_RATIO + m_wheelPhysics.wheelMass) * 9.8f;

            // 転がり抵抗(0.015は，転がり抵抗を推測するマジックナンバー)
            var rollingResistance = velZDir * gravity * 0.015f;

            // 摩擦モデルによる摩擦力の推測
            var baseGrip = m_wheelPhysics.BaseGripCurve.Evaluate(slipAmount);
            var slipRatioGrip = m_wheelPhysics.SlipRatioGripCurve.Evaluate(Mathf.Abs(slipRatio));

            // 摩擦係数(最終決定)
            m_totalGrip = baseGrip * slipRatioGrip * m_gripFactor;

            // 重力 * 摩擦係数 = タイヤが持つ摩擦パワーの最大値
            var frictionForce = velZDir * gravity * m_totalGrip;

            // 摩擦力をベクトル化

            // 三角関数で摩擦力を分配．
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

            // 各力の合計をベクトルに変換
            var totalTireForce = m_dummyWheel.transform.TransformDirection(targetX, 0f, targetZ);

            // サスペンションによる力を計算

            // サスペンションの変化量から回転数の係数を操作する
            var springForce = (m_susCps - m_wheelPhysics.susDst * m_wheelPhysics.targetPos) * m_wheelPhysics.spring;
            var damperForce = (m_susCps - m_susCpsPrev) / Time.deltaTime * m_wheelPhysics.damper;
            var suspentionForce = m_raycastHit.normal * (springForce + damperForce);

            // タイヤに力を加える

            // サスペンションとタイヤのパワーをそれぞれ車体の重心に加える
            m_rigidbody.AddForceAtPosition(totalTireForce, m_dummyWheel.position, ForceMode.Force);
            m_rigidbody.AddForceAtPosition(suspentionForce, transform.position, ForceMode.Force);
        }

        private void UpdateEngineRpm()
        {
            // 現在のタイヤの状態からRPMをどれだけ減衰させるか決定

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

                    // 車が進行方向に対して傾斜しているときはタイヤがスリップしやすくする
                    var dst = Mathf.Abs(m_rawWheelRPM * m_totalGearRatio);
                    var angleRatio = m_wheelPhysics.AngleRatioCurve.Evaluate(Mathf.Abs(m_slipAngle / Mathf.PI * 180));
                    var m_torqueRatio = m_wheelPhysics.TorqueRatioCruve.Evaluate(Mathf.Abs(m_torque));
                    var toRawEngineRpm = TLab.Math.LinerApproach(m_engineRpm, GetFrameLerp(increment) * angleRatio * m_torqueRatio, dst);

                    // エンジンの動力をギアに伝えたことによる回転数の減衰
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
            // タイヤのデバッグ用
            m_lineMaterial = new Material(m_lineMaterial);

            m_dummyWheel = new GameObject("DummyWheel").transform;
            m_dummyWheel.transform.position = this.transform.position;
            m_dummyWheel.transform.parent = this.transform.parent;

            m_rigidbody = GetComponentFromParent<Rigidbody>(this.transform, this.GetType().FullName.ToString());

            // raycastを有効化するために，コライダーを追加
            m_collider = gameObject.AddComponent<SphereCollider>();
            m_collider.center = new Vector3(0.0f, m_wheelPhysics.wheelRadius, 0.0f);
            m_collider.radius = 0.0f;

            m_circleLength = m_wheelPhysics.wheelRadius * 2.0f * Mathf.PI;
        }
    }
}