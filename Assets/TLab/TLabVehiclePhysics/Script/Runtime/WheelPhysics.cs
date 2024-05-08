using UnityEngine;
using TLab.LUTTool;
using TLab.VehiclePhysics.PacejkaTool;

namespace TLab.VehiclePhysics
{
    [CreateAssetMenu(menuName = "TLab/VehiclePhysics/WheelPhysics", fileName = "WheelPhysics")]
    public class WheelPhysics : ScriptableObject
    {
        [SerializeField] public LUT baseGrip;

        [SerializeField] public MultiPacejka longitudinalGrip;
        [SerializeField] public MultiPacejka lateralGrip;

        [SerializeField] public float spring = 75000f;
        [SerializeField] public float damper = 5000f;
        [SerializeField] public float wheelMass = 1f;
        [SerializeField] public float susDst = 0.2f;
        [SerializeField] public float targetPos = 0f;
        [SerializeField] public float wheelRadius = 0.35f;

        public Vector3 amountOfReduction => Vector3.up * (susDst - susCps);

        public float springForce => (susCps - susDst * targetPos) * spring;

        public float damperForce => (susCps - susCpsPrev) / Time.deltaTime * damper;

        public float suspentionFource => springForce + damperForce;

        public float circleLength => wheelRadius * 2f * Mathf.PI;

        public float wheelRpm2Vel => circleLength / 60;

        public float vel2WheelRpm => 60 / circleLength;

        //
        // Suspention
        //

        [HideInInspector] public bool grounded = false;
        [HideInInspector] public float susCps = 0f;
        [HideInInspector] public float susCpsPrev = 0f;
        [HideInInspector] public Color gizmoColor = Color.green;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raycastHit"></param>
        /// <param name="dummyWheel"></param>
        /// <param name="grounded"></param>
        /// <returns></returns>
        public Vector3 UpdateSuspention(RaycastHit raycastHit, Transform dummyWheel, bool grounded)
        {
            this.grounded = grounded;
            if (grounded)
            {
                gizmoColor = Color.green;
                susCpsPrev = susCps;

                var stretchedOut = wheelRadius + susDst;
                var suspentionOrigin = dummyWheel.position;
                susCps = stretchedOut - (raycastHit.point - suspentionOrigin).magnitude;
            }
            else
            {
                gizmoColor = Color.blue;
                susCps = 0f;
            }

            return dummyWheel.localPosition - amountOfReduction;
        }
    }
}
