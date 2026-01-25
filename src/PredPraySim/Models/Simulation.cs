using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace PredPraySim.Models
{
    public class Simulation
    {
        public ShaderConfig shaderConfig;

        public Agent[] agents;

        public Simulation()
        {
            shaderConfig = new ShaderConfig();
            agents = new Agent[shaderConfig.agentsCount];
            InitRandomly();
        }

        private void InitRandomly()
        {
            var rnd = new Random(1);
            for(int i=0; i<agents.Length; i++)
            {
                agents[i] = new Agent();
                agents[i].type = i % 3;
                agents[i].angle = (float)(2 * Math.PI * rnd.NextDouble());
                agents[i].position = new Vector2((float)(shaderConfig.width * rnd.NextDouble()), (float)(shaderConfig.height * rnd.NextDouble()));
            }
        }
    }
}
