
namespace TLab
{
    public static class Math
    {
        public static float LinerApproach(float start, float increment, float dst)
        {
            // A function that approaches a target value in constant increments
            // increment must be positive value

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
