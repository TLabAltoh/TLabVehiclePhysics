using System.Collections.Generic;
using UnityEngine;

namespace TLab.VehiclePhysics.PacejkaTool
{
    [CreateAssetMenu(fileName = "MultiPacejka", menuName = "TLab/VehiclePhysics/PacejkaTool/MultiPacejka")]
    public class MultiPacejka : ScriptableObject
    {
        [SerializeField] public PacejkaDic[] pacejkaDic;

#if UNITY_EDITOR
        [SerializeField] public GraphSettings graphSettings;
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="L"></param>
        /// <param name="H"></param>
        /// <param name="pacejkaDic"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static LerpPacejka LerpPacejka(int L, int H, PacejkaDic[] pacejkaDic, float x)
        {
            int M = (L + H) / 2;
            bool a = x >= pacejkaDic[M].index;
            bool b = x <= pacejkaDic[M + 1].index;

            if (L <= H && a && b)
            {
                LerpPacejka element = new LerpPacejka
                {
                    pacejka0 = pacejkaDic[M].pacejka,
                    pacejka1 = pacejkaDic[M + 1].pacejka,
                    factor = (x - pacejkaDic[M].index) / (pacejkaDic[M + 1].index - pacejkaDic[M].index)
                };

                return element;
            }
            else if (!a && b)
            {
                return LerpPacejka(L, M, pacejkaDic, x);
            }
            else
            {
                return LerpPacejka(M, H, pacejkaDic, x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public LerpPacejka LerpPacejka(float x)
        {
            if (x <= pacejkaDic[0].index)
            {
                LerpPacejka element = new LerpPacejka
                {
                    pacejka0 = pacejkaDic[0].pacejka,
                    pacejka1 = pacejkaDic[0].pacejka,
                    factor = 0f
                };

                return element;
            }
            else if (x >= pacejkaDic[pacejkaDic.Length - 1].index)
            {
                LerpPacejka element = new LerpPacejka
                {
                    pacejka0 = pacejkaDic[pacejkaDic.Length - 1].pacejka,
                    pacejka1 = pacejkaDic[pacejkaDic.Length - 1].pacejka,
                    factor = 1f
                };

                return element;
            }
            else
            {
                return LerpPacejka(0, pacejkaDic.Length - 1, pacejkaDic, x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public float Evaluate(float x, float y)
        {
            // index lerp
            var lerpPacejka = LerpPacejka(x);

            // value lerp
            var value0 = lerpPacejka.pacejka0.Evaluate(y);
            var value1 = lerpPacejka.pacejka1.Evaluate(y);

            return value0 * (1f - lerpPacejka.factor) + value1 * lerpPacejka.factor;
        }
    }
}
