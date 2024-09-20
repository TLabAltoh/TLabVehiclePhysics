using UnityEngine;

namespace TLab.LUTTool
{
    [CreateAssetMenu(menuName = "TLab/LUTTool/LUT")]
    public class LUT : ScriptableObject
    {
        public enum Axis
        {
            X,
            Y,
        };

        public Vector2[] table;

#if UNITY_EDITOR
        public GraphSettings graphSettings;
#endif

        public static float GetMax(Vector2[] table, Axis axis)
        {
            var maxValue = -float.MaxValue;
            switch (axis)
            {
                case Axis.X:
                    for (var i = 0; i < table.Length; i++)
                        if (table[i].x > maxValue)
                            maxValue = table[i].x;
                    break;
                case Axis.Y:
                    for (var i = 0; i < table.Length; i++)
                        if (table[i].y > maxValue)
                            maxValue = table[i].y;
                    break;
            }
            return maxValue;
        }

        public static float GetMin(Vector2[] table, Axis axis)
        {
            var minValue = float.MaxValue;
            switch (axis)
            {
                case Axis.X:
                    for (var i = 0; i < table.Length; i++)
                        if (table[i].x < minValue)
                            minValue = table[i].x;
                    break;
                case Axis.Y:
                    for (var i = 0; i < table.Length; i++)
                        if (table[i].y < minValue)
                            minValue = table[i].y;
                    break;
            }
            return minValue;
        }

        public float GetMax(Axis axis)
        {
            return GetMax(table, axis);
        }

        public float GetMin(Axis axis)
        {
            return GetMin(table, axis);
        }

        public static IndexAndLerpFactor GetIndexAndLerpFactor(int L, int H, Vector2[] table, float x)
        {
            var M = (L + H) / 2;
            var a = x >= table[M].x;
            var b = x <= table[M + 1].x;

            if (L <= H && a && b)
            {
                var candidate = new IndexAndLerpFactor
                {
                    index0 = M,
                    index1 = M + 1,
                    factor = (x - table[M].x) / (table[M + 1].x - table[M].x)
                };
                return candidate;
            }
            else if (!a && b)
                return GetIndexAndLerpFactor(L, M, table, x);
            else
                return GetIndexAndLerpFactor(M, H, table, x);
        }

        public IndexAndLerpFactor GetIndexAndLerpFactor(float x)
        {
            if (x <= table[0].x)
            {
                var candidate = new IndexAndLerpFactor
                {
                    index0 = 0,
                    index1 = 0,
                    factor = 0f
                };
                return candidate;
            }
            else if (x >= table[table.Length - 1].x)
            {
                var candidate = new IndexAndLerpFactor
                {
                    index0 = table.Length - 1,
                    index1 = table.Length - 1,
                    factor = 1f
                };
                return candidate;
            }
            else
                return GetIndexAndLerpFactor(0, table.Length - 1, table, x);
        }

        public float Evaluate(float x)
        {
            var candidate = GetIndexAndLerpFactor(x);
            var y0 = table[candidate.index0].y;
            var y1 = table[candidate.index1].y;
            return y0 * (1f - candidate.factor) + y1 * candidate.factor;
        }
    }
}
