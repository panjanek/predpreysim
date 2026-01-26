using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredPreySim.Utils
{
    public static class MathUtil
    {
        public static float[] Normalize(float[] array, float decay)
        {
            float[] result = new float[array.Length];
            int n = (int)Math.Sqrt(array.Length);
            var sum = array.Sum();
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = decay * array[i] / sum;
                int x = i % n;
                int y = i / n;
                if (array[i] != array[y * n + (n - x - 1)] || array[i] != array[(n - y - 1) * n + x] || array[i] != array[(n - y - 1) * n + (n - x - 1)])
                    throw new Exception("Kernel not symmetric!");
            }

            return result;
        }

        public static double NextGaussian(Random rng, double mean = 0.0, double stdDev = 1.0)
        {
            // Uniform(0,1] random doubles
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();

            // Standard normal (0,1)
            double randStdNormal =
                Math.Sqrt(-2.0 * Math.Log(u1)) *
                Math.Cos(2.0 * Math.PI * u2);

            return mean + stdDev * randStdNormal;
        }
    }
}
