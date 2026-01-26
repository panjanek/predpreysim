using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Mutate(float[] network, int offset, Random rnd)
        {
            int i = rnd.Next(Size);
            if ((i >= 0 && i < 120) || (i >= 128 && i < 144))
                network[offset + i] = (float)(rnd.NextDouble() * 2 - 1);
            else
                network[offset + i] = (float)(rnd.NextDouble() * 1 - 0.5);
        }
    }
}
