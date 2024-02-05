using UnityEngine;
using TLab.LUTUtil;

namespace TLab
{
    [CreateAssetMenu(menuName = "TLab/LUT/MultiLUT")]
    public class MultiLUT : ScriptableObject
    {
        public LUTDic[] lutDic;

#if UNITY_EDITOR
        [SerializeField]
        [Range(0, 10)]
        public int div = 5;

        [SerializeField]
        [Range(1, 10)]
        public int xAccuracy = 1;

        [SerializeField]
        [Range(1, 10)]
        public int yAccuracy = 1;
#endif

        public static LerpLUT LerpLUT(int L, int H, LUTDic[] lutDic, float x)
        {
            int M = (L + H) / 2;
            bool a = x >= lutDic[M].index;
            bool b = x <= lutDic[M + 1].index;

            if (L <= H && a && b)
            {
                LerpLUT element = new LerpLUT
                {
                    lut0 = lutDic[M].lut,
                    lut1 = lutDic[M + 1].lut,
                    factor = (x - lutDic[M].index) / (lutDic[M + 1].index - lutDic[M].index)
                };

                return element;
            }
            else if (!a && b)
            {
                return LerpLUT(L, M, lutDic, x);
            }
            else
            {
                return LerpLUT(M, H, lutDic, x);
            }
        }

        public LerpLUT LerpLUT(float x)
        {
            if (x <= lutDic[0].index)
            {
                LerpLUT element = new LerpLUT
                {
                    lut0 = lutDic[0].lut,
                    lut1 = lutDic[0].lut,
                    factor = 0f
                };

                return element;
            }
            else if (x >= lutDic[lutDic.Length - 1].index)
            {
                LerpLUT element = new LerpLUT
                {
                    lut0 = lutDic[lutDic.Length - 1].lut,
                    lut1 = lutDic[lutDic.Length - 1].lut,
                    factor = 1f
                };

                return element;
            }
            else
            {
                return LerpLUT(0, lutDic.Length - 1, lutDic, x);
            }
        }

        public float Evaluate(float x, float y)
        {
            /**
             * index lerp
             */
            var lerpLUT = LerpLUT(x);

            /**
             * value lerp
             */
            var value0 = lerpLUT.lut0.Evaluate(y);
            var value1 = lerpLUT.lut1.Evaluate(y);

            return value0 * (1f - lerpLUT.factor) + value1 * lerpLUT.factor;
        }
    }
}
