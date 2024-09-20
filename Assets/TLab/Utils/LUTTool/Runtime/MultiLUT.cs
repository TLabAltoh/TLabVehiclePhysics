using UnityEngine;

namespace TLab.LUTTool
{
    [CreateAssetMenu(menuName = "TLab/LUTTool/MultiLUT")]
    public class MultiLUT : ScriptableObject
    {
        [System.Serializable]
        public class Element
        {
            public float index;
            public Color color = Color.red;
            public LUT lut;
        }

        public Element[] table;

#if UNITY_EDITOR
        public GraphSettings graphSettings;
#endif

        public static LUTAndLerpFactor GetLUTAndLerpFactor(int L, int H, Element[] table, float x)
        {
            var M = (L + H) / 2;
            var a = x >= table[M].index;
            var b = x <= table[M + 1].index;

            if (L <= H && a && b)
            {
                var element = new LUTAndLerpFactor
                {
                    lut0 = table[M].lut,
                    lut1 = table[M + 1].lut,
                    factor = (x - table[M].index) / (table[M + 1].index - table[M].index)
                };
                return element;
            }
            else if (!a && b)
                return GetLUTAndLerpFactor(L, M, table, x);
            else
                return GetLUTAndLerpFactor(M, H, table, x);
        }

        public LUTAndLerpFactor GetLUTAndLerpFactor(float x)
        {
            if (x <= table[0].index)
            {
                var element = new LUTAndLerpFactor
                {
                    lut0 = table[0].lut,
                    lut1 = table[0].lut,
                    factor = 0f
                };
                return element;
            }
            else if (x >= table[table.Length - 1].index)
            {
                var element = new LUTAndLerpFactor
                {
                    lut0 = table[table.Length - 1].lut,
                    lut1 = table[table.Length - 1].lut,
                    factor = 1f
                };
                return element;
            }
            else
                return GetLUTAndLerpFactor(0, table.Length - 1, table, x);
        }

        public float Evaluate(float x, float y)
        {
            var element = GetLUTAndLerpFactor(x);
            var value0 = element.lut0.Evaluate(y);
            var value1 = element.lut1.Evaluate(y);
            return value0 * (1f - element.factor) + value1 * element.factor;
        }
    }
}
