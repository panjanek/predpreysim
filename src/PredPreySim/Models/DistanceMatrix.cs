using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredPreySim.Models.NN;

namespace PredPreySim.Models
{
    public class DistanceMatrix
    {
        private double[,] matrix;

        private Dictionary<int, int> map;

        public DistanceMatrix(Simulation sim, List<int> indexes, Func<INeuralNetwork, float[], int, int, double> norm) 
        {
            map = new Dictionary<int, int>();
            matrix = new double[indexes.Count, indexes.Count];
            for (int i = 0; i < indexes.Count; i++)
            {
                map[indexes[i]] = i;
                for (int j = 0; j <= i; j++)
                {
                    if (i == j)
                        matrix[i, j] = 0;
                    else
                    {
                        var dist = norm(sim.nn, sim.network, sim.agents[indexes[i]].nnOffset, sim.agents[indexes[j]].nnOffset);
                        matrix[i, j] = dist;
                        matrix[j, i] = dist;
                    }
                }
            }
        }

        public double GetDistance(int agent1Idx, int agent2Idx)
        {
            return matrix[map[agent1Idx], map[agent2Idx]];
        }

        public double GetDiversity()
        {
            double sum = 0;
            int n = map.Count;
            for(int i=0; i<n; i++)
                for(int j=0; j<i; j++)
                    sum += matrix[i, j];
            return sum / (n * (n - 1) / 2);
        }

        public static double L2Distance(INeuralNetwork nn, float[] network, int offset1, int offset2)
        {
            double sum = 0;
            for (int i = 0; i < nn.Size; i++)
            {
                var d = (double)network[offset1 + i] - (double)network[offset2 + i];
                sum += d * d;
            }

            return sum / nn.Size;
        }

        public static double BehavioralDistance(INeuralNetwork nn, float[] network, int offset1, int offset2)
        {
            int samplesCount = 25;
            double totalSum = 0;
            for(int s=0; s< samplesCount; s++)
            {
                double sum = 0;
                var sample = nn.GetInputSample(s);
                var out1 = nn.Evaluate(network, offset1, sample);
                var out2 = nn.Evaluate(network, offset2, sample);
                for (int i = 0; i < out1.Length; i++)
                {
                    var d = (double)out1[i] - (double)out2[i];
                    sum += d * d;
                }

                totalSum += sum / out1.Length;
            }

            return totalSum / samplesCount;
        }
    }
}
