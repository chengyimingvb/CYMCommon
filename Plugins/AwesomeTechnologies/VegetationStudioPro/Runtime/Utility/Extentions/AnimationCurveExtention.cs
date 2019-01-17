using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public static class AnimationCurveExtention
    {
        public static float[] GenerateCurveArray(this AnimationCurve self)
        {
            float[] returnArray = new float[256];
            for (int j = 0; j <= 255; j++)
                returnArray[j] = self.Evaluate(j / 256f);

            return returnArray;
        }
    }
}
