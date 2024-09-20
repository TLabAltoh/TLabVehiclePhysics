using UnityEngine;

namespace TLab.VehiclePhysics.PacejkaTool
{
    [CreateAssetMenu(fileName = "Pacejka", menuName = "TLab/VehiclePhysics/PacejkaTool/Pacejka")]
    public class Pacejka : ScriptableObject
    {
        public float d = 1f;
        public float c = 1f;
        public float b = 0.8f;
        public float e = -2.0f;

#if UNITY_EDITOR
        [SerializeField] public GraphSettings graphSettings;
#endif

        public float Evaluate(float x)
        {
            return d * Mathf.Sin(c * Mathf.Atan(b * x - e * Mathf.Atan(b * x - Mathf.Atan(b * x))));
        }
    }
}
