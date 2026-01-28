using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
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

        public float decay = 0.99f;

        public float initialEnergy = 300;

        public int step;

        private INeuralNetwork nn;

        private Random rnd = new Random(1);

        public List<Stats> stats;

        public Simulation()
        {
            shaderConfig = new ShaderConfig();
            agents = new Agent[shaderConfig.agentsCount];
            nn = new Network_15_8_2();
            InitRandomly(0.6, 0.1);
            kernel = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decay);
            stats = new List<Stats>();
        }

        private void InitRandomly(double plants, double predators)
        {
            int networksCount = 0;
            for(int i=0; i<agents.Length; i++)
            {
                var r = rnd.NextDouble();

                agents[i] = new Agent();
                agents[i].type = r < plants ? 0 : (r < (1 - predators) ? 1 : 2);
                agents[i].energy = initialEnergy;
                agents[i].angle = (float)(2 * Math.PI * rnd.NextDouble());
                agents[i].position = new Vector2((float)(shaderConfig.width * rnd.NextDouble()), (float)(shaderConfig.height * rnd.NextDouble()));

                if (agents[i].type > 0)
                    networksCount++;
            }

            network = new float[nn.Size * networksCount];
            int offset = 0;
            for (int i = 0; i < agents.Length; i++)
                if (agents[i].type > 0)
                {
                    nn.Init(network, offset, rnd);
                    agents[i].nnOffset = offset;
                    offset += nn.Size;
                }
        }

        public void ChangeEpoch()
        {
            
            var blue = agents.Where(a => a.type == 1).OrderByDescending(a=>a.energy).ToList();
            var minBlueMeals = blue.Min(a => a.meals);
            var maxBlueMeals = blue.Max(a => a.meals);

            var red = agents.Where(a => a.type == 2).OrderByDescending(a => a.energy).ToList();
            var minRedMeals = red.Min(a => a.meals);
            var maxRedMeals = red.Max(a => a.meals);

            


            var ranking = agents.Select((a, i) => new { index = i, agent = a, fitness = a.meals * 2 - a.deaths * 5 - a.energySpent * 0.01 }).ToList();

            var minBlueScore = ranking.Where(x => x.agent.type == 1).Min(a => a.fitness);
            var maxBlueScore = ranking.Where(x => x.agent.type == 1).Max(a => a.fitness);
            var minRedScore = ranking.Where(x => x.agent.type == 2).Min(a => a.fitness);
            var maxRedScore = ranking.Where(x => x.agent.type == 2).Max(a => a.fitness);

            var allBlueCount = agents.Count(a => a.type == 1);
            var topBlue = ranking.Where(x => x.agent.type == 1).OrderByDescending(x => x.fitness).Take(allBlueCount / 10).ToList();
            var topBlueIds = topBlue.Select(x=>x.index).ToList();
            var downBlueIds = ranking.Where(x => x.agent.type == 1).OrderBy(x => x.fitness).Take(allBlueCount / 2).Select(x => x.index).ToList();
            if (topBlueIds.Intersect(downBlueIds).Count() > 0)
                throw new Exception("!");

            Breed(topBlueIds, downBlueIds);

            var allRedCount = agents.Count(a => a.type == 2);
            var topRed = ranking.Where(x => x.agent.type == 2).OrderByDescending(x => x.fitness).Take(allRedCount / 10).ToList();
            var topRedIds = topRed.Select(x => x.index).ToList();
            var downRedIds = ranking.Where(x => x.agent.type == 2).OrderBy(x => x.fitness).Take(allRedCount / 2).Select(x => x.index).ToList();
            if (topRedIds.Intersect(downRedIds).Count() > 0)
                throw new Exception("!");

            Breed(topRedIds, downRedIds);

            stats.Add(new Stats()
            {
                time = shaderConfig.t,
                topBlueAvgFitness = topBlue.Average(x=>x.fitness),
                topRedAvgFitness = topRed.Average(x=>x.fitness),
                topBlueAvgMeals = topBlue.Average(x => x.agent.meals),
                topRedAvgMeals = topRed.Average(x => x.agent.meals),
                topBlueAvgDeaths = topBlue.Average(x => x.agent.deaths),
                topRedAvgDeaths = topRed.Average(x => x.agent.deaths),

                topBlueMedFitness = topBlue.Median(x => x.fitness),
                topRedMedFitness = topRed.Median(x => x.fitness),
                topBlueMedMeals = topBlue.Median(x => x.agent.meals),
                topRedMedMeals = topRed.Median(x => x.agent.meals),
                topBlueMedDeaths = topBlue.Median(x => x.agent.deaths),
                topRedMedDeaths = topRed.Median(x => x.agent.deaths),
            });
        }

        private void Breed(List<int> parents, List<int> spaces)
        {
            int p = 0;
            foreach(var childIdx in spaces)
            {
                var parentIdx = parents[p % parents.Count];
                p++;

                agents[childIdx].state = 0;
                agents[childIdx].age = 0;
                agents[childIdx].meals = 0;
                agents[childIdx].deaths = 0;
                agents[childIdx].energySpent = 0;
                agents[childIdx].energy = initialEnergy;
                agents[childIdx].position = agents[parentIdx].position + new Vector2((float)rnd.NextDouble() * 10 - 5, (float)rnd.NextDouble() * 10 - 5);
                agents[childIdx].angle = (float)(2 * Math.PI * rnd.NextDouble());

                Array.Copy(network, agents[parentIdx].nnOffset, network, agents[childIdx].nnOffset, nn.Size);
                if (rnd.NextDouble() < 0.5)
                    nn.Mutate(network, agents[childIdx].nnOffset, rnd);
            }
        }
    }
}
