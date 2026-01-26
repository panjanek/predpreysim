using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Mathematics;
using PredPreySim.Models.NN;
using PredPreySim.Utils;

namespace PredPreySim.Models
{
    public class Simulation
    {
        public ShaderConfig shaderConfig;

        public Agent[] agents;

        public float[] network;

        public float[] kernel;

        public float decay = 0.98f;

        public Simulation()
        {
            shaderConfig = new ShaderConfig();
            agents = new Agent[shaderConfig.agentsCount];
            InitRandomly();
            kernel = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decay);
        }

        private void InitRandomly()
        {
            INeuralNetwork nn = new Network_15_8_2();
            network = new float[nn.Size * 3];
            nn.Init(network, 0 * nn.Size, 0);
            nn.Init(network, 1 * nn.Size, 1);
            nn.Init(network, 2 * nn.Size, 2);

            var rnd = new Random(1);
            for(int i=0; i<agents.Length; i++)
            {
                agents[i] = new Agent();
                agents[i].type = i % 3;
                agents[i].angle = (float)(2 * Math.PI * rnd.NextDouble());
                agents[i].position = new Vector2((float)(shaderConfig.width * rnd.NextDouble()), (float)(shaderConfig.height * rnd.NextDouble()));
                agents[i].nnOffset = agents[i].type * nn.Size;
            }
        }
    }
}
