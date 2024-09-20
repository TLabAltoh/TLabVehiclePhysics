using UnityEngine;

namespace TLab.VehiclePhysics.PacejkaTool
{
    [CreateAssetMenu(fileName = "MultiPacejka", menuName = "TLab/VehiclePhysics/PacejkaTool/MultiPacejka")]
    public class MultiPacejka : ScriptableObject
    {
        [System.Serializable]
        public class Element
        {
            public float index;
            public Color color = Color.red;
            public Pacejka pacejka;
        }

        [SerializeField] public Element[] table;

#if UNITY_EDITOR
        [SerializeField] public GraphSettings graphSettings;
#endif

        public static PacejkaAndLerpFactor GetPacejkaAndLerpFactor(int L, int H, Element[] table, float x)
        {
            var M = (L + H) / 2;
            var a = x >= table[M].index;
            var b = x <= table[M + 1].index;

            if (L <= H && a && b)
            {
                var candidate = new PacejkaAndLerpFactor
                {
                    pacejka0 = table[M].pacejka,
                    pacejka1 = table[M + 1].pacejka,
                    factor = (x - table[M].index) / (table[M + 1].index - table[M].index)
                };
                return candidate;
            }
            else if (!a && b)
                return GetPacejkaAndLerpFactor(L, M, table, x);
            else
                return GetPacejkaAndLerpFactor(M, H, table, x);
        }

        public PacejkaAndLerpFactor GetPacejkaAndLerpFactor(float x)
        {
            if (x <= table[0].index)
            {
                var candidate = new PacejkaAndLerpFactor
                {
                    pacejka0 = table[0].pacejka,
                    pacejka1 = table[0].pacejka,
                    factor = 0f
                };
                return candidate;
            }
            else if (x >= table[table.Length - 1].index)
            {
                var candidate = new PacejkaAndLerpFactor
                {
                    pacejka0 = table[table.Length - 1].pacejka,
                    pacejka1 = table[table.Length - 1].pacejka,
                    factor = 1f
                };
                return candidate;
            }
            else
                return GetPacejkaAndLerpFactor(0, table.Length - 1, table, x);
        }

        public float Evaluate(float x, float y)
        {
            var candidate = GetPacejkaAndLerpFactor(x);
            var value0 = candidate.pacejka0.Evaluate(y);
            var value1 = candidate.pacejka1.Evaluate(y);
            return value0 * (1f - candidate.factor) + value1 * candidate.factor;
        }
    }
}
