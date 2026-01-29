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

        // mutate randomly selected weights
        public void Mutate(float[] network, int offset, Random rnd, double changedWeightsRatio, double stdDev)
        {
            int offs2 = hidden * inputs + hidden;
            for (int i = 0; i < Size; i++)
            {
                if (rnd.NextDouble() <= changedWeightsRatio)
                {
                    double stdMult = 1.0;
                    if (i >= inputs * hidden && i < inputs * hidden + hidden)
                        stdMult = 0.5; // mutate bias of 1st layer by half amount
                    if (i >= offs2 + hidden * outputs)
                        stdMult = 0.25; // mutate bias of output layer by quarter amount
                    double delta = MathUtil.NextGaussian(rnd, 0.0, stdDev * stdMult);
                    network[offset + i] += (float)delta;
                }
            }
        }

        // mutate all inputs of one hidden layer neuron
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

                double biasDelta = MathUtil.NextGaussian(rnd, 0.0, stdDev*0.5);
                network[offset + inputs * hidden + h] += (float)biasDelta;
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

                double biasDelta = MathUtil.NextGaussian(rnd, 0.0, stdDev * 0.5);
                network[offset + offs2 + hidden*outputs + o] += (float)biasDelta;
            }
        }

        public void CrossOver(float[] network, int parent1Offset, int parent2Offset, int childOffset, Random rnd)
        {
            var decision1 = rnd.NextDouble();
            int offs2 = hidden * inputs + hidden;
            if (decision1 < 0.5) // with probability 50%: get 1st laver from parent1, second layer from parent 2 (parents already random)
            {
                for (int i = 0; i < Size; i++)
                    network[childOffset + i] = i < offs2 ? network[parent1Offset + i] : network[parent2Offset + i];
            }
            else if (decision1 < 0.9)  // with probability 40%: combine neurons from parents randomly
            {
                for (int h = 0; h < hidden; h++)
                {
                    var parentOffset = rnd.NextDouble() < 0.5 ? parent1Offset : parent2Offset;
                    network[childOffset + hidden * inputs + h] = network[parentOffset + hidden * inputs + h]; //pick bias
                    for (int i = 0; i < inputs; i++)
                        network[childOffset + h * inputs + i] = network[parentOffset + h * inputs + i]; // pick input weights
                    for (int o = 0; o < outputs; o++)
                        //network[childOffset + offs2 + h * outputs + o] = network[parentOffset + offs2 + h * outputs + o]; // pick output weights
                        network[childOffset + offs2 + o * hidden + h] = network[parentOffset + offs2 + o * hidden + h]; // pick output weights
                }

                var decision2 = rnd.NextDouble();
                for (int o = 0; o < outputs; o++)
                {
                    int outputBiasOffs = offs2 + hidden * outputs + o;
                    if (decision2 < 0.50) // 50% of the times: average output biases:
                        network[childOffset + outputBiasOffs] = 0.5f * (network[parent1Offset + outputBiasOffs] + network[parent2Offset + outputBiasOffs]);
                    else if (decision2 < 0.75) //25% of times: pick output biases from parents randomly
                    {
                        var parentOffset = rnd.NextDouble() < 0.5 ? parent1Offset : parent2Offset;
                        network[childOffset + outputBiasOffs] = network[parentOffset + outputBiasOffs];
                    } // 25% of times: leave output biases unchanged
                }
            }
            else //with probability 10%: combine weights of parents randomly - may be destructive
            {
                for (int i = 0; i < Size; i++)
                    network[childOffset + i] = rnd.NextDouble() < 0.5 ? network[parent1Offset + i] : network[parent2Offset + i];
            }
        }
    }
}
