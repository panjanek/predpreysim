using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredPreySim.Models
{
    public class StartNewSimulationParameters
    {
        public int width;

        public int height;

        public double plantsRatio;

        public double predatorsRatio;

        public int agentsCount;

        public bool fixedSeed;

        public bool useExistingAgents;

        public bool loadAgentsFromFiles;

        public float decayGreen;

        public float decayBlue;

        public float decayRed;

        public float blueMaxVelocity;

        public float redMaxVelocity;

        public List<string> fileNames = new List<string>();

        public List<Simulation> sources = new List<Simulation>();
    }
}
