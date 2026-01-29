using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredPreySim.Utils;

namespace PredPreySim.Models.NN
{
    public class NeuralNetwork : INeuralNetwork
    {
        public int Size => inputs * hidden + hidden + hidden * outputs + outputs;

        private int inputs;

        private int hidden;

        private int outputs;
        public NeuralNetwork(int inputs, int hidden, int outputs)
        {
            this.inputs = inputs;
            this.hidden = hidden;
            this.outputs = outputs;
        }

        public void Init(float[] network, int offset, Random rnd)
        {
            for (int i = 0; i < inputs * hidden; i++) //weights 1
                network[offset + i] = (float)(rnd.NextDouble() * 2 - 1);
            for (int i = inputs * hidden; i < inputs * hidden + hidden; i++) //biases 1
                network[offset + i] = (float)(rnd.NextDouble() * 1 - 0.5);
            int offs2 = hidden * inputs + hidden;
            for (int i = offs2; i < offs2 + hidden * outputs; i++)  //weights 2
                network[offset + i] = (float)(rnd.NextDouble() * 2 - 1);
            for (int i = offs2 + hidden * outputs; i < Size; i++)
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
            int h = rnd.Next(hidden + outputs);
            if (h < hidden) //1st layer
            {
                for (int i = 0; i < inputs; i++)
                {
                    double delta = MathUtil.NextGaussian(rnd, 0.0, stdDev);
                    network[offset + h * inputs + i] += (float)delta;
                }
            }
            else //2nd layer
            {
                var o = h - hidden;
                int offs2 = hidden * inputs + hidden;
                for (int i = 0; i < hidden; i++)
                {
                    double delta = MathUtil.NextGaussian(rnd, 0.0, stdDev);
                    network[offset + offs2 + o*hidden + i] += (float)delta;
                }
            }
        }

        public void Cross(float[] network, int parent1Offset, int parent2Offset, int childOffset, Random rnd)
        {
            for (int i = 0; i < Size; i++)
                network[childOffset + i] = rnd.NextDouble() < 0.5 ? network[parent1Offset + i] : network[parent2Offset + i];
        }
    }
}
