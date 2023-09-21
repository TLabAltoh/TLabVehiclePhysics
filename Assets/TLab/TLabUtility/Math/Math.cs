
namespace TLab
{
    public static class Math
    {
        /// <summary>
        /// Approaches the desired value in linear increments
        /// </summary>
        /// <param name="start"></param>
        /// <param name="increment"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static float LinerApproach(float start, float increment, float dst)
        {
            float incrementDir = dst - start;

            if (incrementDir > 0)
            {
                // increment approache
                return start + increment < dst ? start + increment : dst;
            }
            else if (incrementDir < 0)
            {
                // decrement approache
                return start - increment > dst ? start - increment : dst;
            }
            else
            {
                return dst;
            }
        }
    }
}
