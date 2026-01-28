using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredPreySim.Utils;

namespace PredPreySim.Models.NN
{
    public class Network_15_8_2 : INeuralNetwork
    {
        public int Size => 146;

        public void Init(float[] network, int offset, Random rnd)
        {
            for (int i = 0; i < 120; i++)
                network[offset + i] = (float)(rnd.NextDouble() * 2 - 1);
            for (int i = 120; i < 128; i++)
                network[offset + i] = (float)(rnd.NextDouble() * 1 - 0.5);
            for (int i = 128; i < 144; i++)
                network[offset + i] = (float)(rnd.NextDouble() * 2 - 1);
            for (int i = 144; i < 146; i++)
                network[offset + i] = (float)(rnd.NextDouble() * 1 - 0.5);
        }

        public void Mutate(float[] network, int offset, Random rnd, double changedWeightsRatio, double stdDev)
        {
            for (int i = 0; i < Size; i++)
            {
                if (rnd.NextDouble() <= changedWeightsRatio)
                {
                    double delta = MathUtil.NextGaussian(rnd, 0.0, stdDev);
                    network[offset + i] += (float)delta;
                }
            }
        }

        public void MutateAllIncomming(float[] network, int offset, Random rnd, double stdDev)
        {
            int h = rnd.Next(8);
            for(int i=0; i<15; i++)
            {
                double delta = MathUtil.NextGaussian(rnd, 0.0, stdDev);
                network[offset + h*15 + i] += (float)delta;
            }
        }

        public void Cross(float[] network, int parent1Offset, int parent2Offset, int childOffset, Random rnd)
        {
            for (int i = 0; i < Size; i++)
                network[childOffset + i] = rnd.NextDouble() < 0.5 ? network[parent1Offset + i] : network[parent2Offset + i];
        }
    }
}
