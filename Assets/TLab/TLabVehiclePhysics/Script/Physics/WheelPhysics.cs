using UnityEngine;

namespace TLab.VehiclePhysics
{
    [CreateAssetMenu()]
    public class WheelPhysics : ScriptableObject
    {
        [Tooltip("Grip curve of the base (ƒÊ/ms)")]
        public LUT baseGrip;

        [Tooltip("Grip Carp based on pecejka magic formula (ƒÊ/slipRatio)")]
        public LUT slipRatioVsGrip;

        public LUT slipAngleVsLerpRatio;

        public LUT torqueVsLerpRatio;

        public Vector3 amountOfReduction => Vector3.up * (susDst - susCps);

        public float springForce => (susCps - susDst * targetPos) * spring;

        public float damperForce => (susCps - susCpsPrev) / Time.deltaTime * damper;

        public float suspentionFource => springForce + damperForce;

        public float circleLength => wheelRadius * 2f * Mathf.PI;

        public float wheelRpm2Vel => circleLength / 60;

        public float vel2WheelRpm => 60 / circleLength;

        public float spring = 75000f;
        public float damper = 5000f;
        public float wheelMass = 1f;
        public float susDst = 0.2f;
        public float targetPos = 0f;
        public float wheelRadius = 0.35f;

        // Suspention
        [HideInInspector] public bool grounded = false;
        [HideInInspector] public float susCps = 0f;
        [HideInInspector] public float susCpsPrev = 0f;
        [HideInInspector] public Color gizmoColor = Color.green;

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
