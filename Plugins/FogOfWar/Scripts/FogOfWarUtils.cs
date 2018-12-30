using UnityEngine;

namespace FoW
{
    public static class FogOfWarUtils
    {
        // Returns clockwise angle [-180-180]
        public static float ClockwiseAngle(Vector2 from, Vector2 to)
        {
            float angle = Vector2.Angle(from, to);
            if (Vector2.Dot(from, new Vector2(to.y, to.x)) < 0.0f)
                angle = -angle;
            return angle;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static void SetKeywordEnabled(this Material material, string keyword, bool enable)
        {
            if (enable)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }
    }
}