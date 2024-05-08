using UnityEngine;

namespace TLab.VehiclePhysics.PacejkaTool
{
    [CreateAssetMenu(fileName = "Pacejka", menuName = "TLab/VehiclePhysics/PacejkaTool/Pacejka")]
    public class Pacejka : ScriptableObject
    {
        [SerializeField] public float d = 1f;
        [SerializeField] public float c = 1f;
        [SerializeField] public float b = 0.8f;
        [SerializeField] public float e = -2.0f;

#if UNITY_EDITOR
        [SerializeField] public GraphSettings graphSettings;
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float Evaluate(float x)
        {
            return d * Mathf.Sin(c * Mathf.Atan(b * x - e * Mathf.Atan(b * x - Mathf.Atan(b * x))));
        }
    }
}
