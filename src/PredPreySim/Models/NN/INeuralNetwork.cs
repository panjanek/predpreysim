using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredPreySim.Models.NN
{
    public interface INeuralNetwork
    {
        int Size { get; }

        void Init(float[] network, int offset, int seed);
    }
}
