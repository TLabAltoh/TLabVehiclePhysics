using UnityEngine;

namespace TLab.LUTTool
{
    [CreateAssetMenu(menuName = "TLab/LUTTool/LUT")]
    public class LUT : ScriptableObject
    {
        [SerializeField] public Vector2[] values;

#if UNITY_EDITOR
        [SerializeField] public GraphSettings graphSettings;

        [SerializeField] public bool fixX = true;

        [SerializeField] public bool fixY = true;
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static float GetMax(Vector2[] values, int axis)
        {
            float maxValue = -float.MaxValue;
            switch (axis)
            {
                case 0:
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].x > maxValue)
                        {
                            maxValue = values[i].x;
                        }
                    }
                    break;
                case 1:
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].y > maxValue)
                        {
                            maxValue = values[i].y;
                        }
                    }
                    break;
                default:
                    Debug.LogError("axis is invalid.");
                    maxValue = 0f;
                    break;
            }

            return maxValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static float GetMin(Vector2[] values, int axis)
        {
            float minValue = float.MaxValue;
            switch (axis)
            {
                case 0:
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].x < minValue)
                        {
                            minValue = values[i].x;
                        }
                    }
                    break;
                case 1:
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].y < minValue)
                        {
                            minValue = values[i].y;
                        }
                    }
                    break;
                default:
                    Debug.LogError("axis is invalid.");
                    minValue = 0f;
                    break;
            }

            return minValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="L"></param>
        /// <param name="H"></param>
        /// <param name="values"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static LerpElement LerpFactor(int L, int H, Vector2[] values, float x)
        {
            int M = (L + H) / 2;
            bool a = x >= values[M].x;
            bool b = x <= values[M + 1].x;

            if (L <= H && a && b)
            {
                LerpElement element = new LerpElement
                {
                    index0 = M,
                    index1 = M + 1,
                    factor = (x - values[M].x) / (values[M + 1].x - values[M].x)
                };

                return element;
            }
            else if (!a && b)
            {
                return LerpFactor(L, M, values, x);
            }
            else
            {
                return LerpFactor(M, H, values, x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public LerpElement LerpFactor(float x)
        {
            if (x <= values[0].x)
            {
                LerpElement element = new LerpElement
                {
                    index0 = 0,
                    index1 = 0,
                    factor = 0f
                };

                return element;
            }
            else if (x >= values[values.Length - 1].x)
            {
                LerpElement element = new LerpElement
                {
                    index0 = values.Length - 1,
                    index1 = values.Length - 1,
                    factor = 1f
                };

                return element;
            }
            else
            {
                return LerpFactor(0, values.Length - 1, values, x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float Evaluate(float x)
        {
            var lerpElement = LerpFactor(x);

            return values[lerpElement.index0].y * (1f - lerpElement.factor) +
                   values[lerpElement.index1].y * lerpElement.factor;
        }
    }
}
