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

        public int generation;

        private INeuralNetwork nn;

        private Random rnd = new Random(1);

        public List<Stats> stats;

        public Simulation()
        {
            shaderConfig = new ShaderConfig();
            agents = new Agent[shaderConfig.agentsCount];
            nn = new NeuralNetwork(15, 8, 2);
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
            generation++;
            var ranking = agents.Select((a, i) => new { index = i, agent = a, fitness = a.Fitness() }).Where(a=>a.agent.type > 0).ToList();

            var allBlueCount = agents.Count(a => a.type == 1);
            var allBlue = ranking.Where(x => x.agent.type == 1);
            var topBlue = allBlue.OrderByDescending(x => x.fitness).Take(allBlueCount / 10).ToList(); //these will breed
            var topBlueIds = topBlue.Select(x=>x.index).ToList();
            var downBlueIds = allBlue.OrderBy(x => x.fitness).Take(allBlueCount / 2).Select(x => x.index).ToList(); //this will be replaced with newly created agents
            if (topBlueIds.Intersect(downBlueIds).Count() > 0)
                throw new Exception("!");

            Breed(topBlueIds, downBlueIds);

            var allRedCount = agents.Count(a => a.type == 2);
            var allRed = ranking.Where(x => x.agent.type == 2);
            var topRed = allRed.OrderByDescending(x => x.fitness).Take(allRedCount / 10).ToList(); //these will breed
            var topRedIds = topRed.Select(x => x.index).ToList();
            var downRedIds = allRed.OrderBy(x => x.fitness).Take(allRedCount / 2).Select(x => x.index).ToList(); //this will be replaced with newly created agents
            if (topRedIds.Intersect(downRedIds).Count() > 0)
                throw new Exception("!");

            Breed(topRedIds, downRedIds);

            // record stats
            stats.Add(new Stats()
            {
                time = shaderConfig.t,
                topBlueAvgFitness = topBlue.Average(x => x.fitness),
                topRedAvgFitness = topRed.Average(x => x.fitness),
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

                topBlueMealsPerAge = topBlue.Average(x => x.agent.age == 0 ? 0 : 1.0 * x.agent.meals / x.agent.age),
                topRedMealsPerAge = topRed.Average(x => x.agent.age == 0 ? 0 : 1.0 * x.agent.meals / x.agent.age),

                topBlueAvgAge = topBlue.Average(x => x.agent.age * 1.0),
                topRedAvgAge = topRed.Average(x => x.agent.age * 1.0),

                plantsCount = agents.Where(a => a.type == 0 && a.state == 0).Count()
            });

            //highlight best
            var bestBlueIdx = topBlue.First().index;
            var bestRedIdx = topRed.First().index;
            for(int i=0; i<agents.Length; i++)
            {
                agents[i].flag = (i == bestBlueIdx || i == bestRedIdx) ? 1 : 0;
            }

        }

        // Take agents from "parents" indexes in agents array and breed. Overwrite agents from "spaces" indexes with newly created. 
        private void Breed(List<int> parents, List<int> spaces)
        {
            foreach(var childIdx in spaces)
            {
                var parent1Idx = parents[rnd.Next(parents.Count)];

                agents[childIdx].state = 0;
                agents[childIdx].age = 0;
                agents[childIdx].meals = 0;
                agents[childIdx].deaths = 0;
                agents[childIdx].energySpent = 0;
                agents[childIdx].energy = initialEnergy;
                agents[childIdx].position = agents[parent1Idx].position + new Vector2((float)rnd.NextDouble() * 10 - 5, (float)rnd.NextDouble() * 10 - 5);
                agents[childIdx].angle = (float)(2 * Math.PI * rnd.NextDouble());

                var decision1 = rnd.NextDouble();
                if (decision1 < 0.15) // 15% of times: copy without changing
                {
                    Array.Copy(network, agents[parent1Idx].nnOffset, network, agents[childIdx].nnOffset, nn.Size);
                }
                else if (decision1 < 0.9) // 75% of times: mutate, single parent
                {
                    Array.Copy(network, agents[parent1Idx].nnOffset, network, agents[childIdx].nnOffset, nn.Size);
                    double mutationAmplification = 2; //for tuning
                    double decision2 = rnd.NextDouble();
                    if (decision2 < 0.6) //60% - mutate slightly
                        nn.Mutate(network, agents[childIdx].nnOffset, rnd, 0.01 * mutationAmplification, 0.05 * mutationAmplification);
                    else if (decision2 < 0.95) //35% - mutate mildly
                        nn.Mutate(network, agents[childIdx].nnOffset, rnd, 0.05 * mutationAmplification, 0.15 * mutationAmplification);
                    else //5% - mutate strong all inputs of one hidden neuron
                        nn.MutateAllIncomming(network, agents[childIdx].nnOffset, rnd, 0.3 * mutationAmplification);
                }
                else // 10% of times: cross-over, two parents
                {
                    //var parent2Idx = parents[rnd.Next(parents.Count)];
                    int parent2Idx;
                    do
                        parent2Idx = parents[rnd.Next(parents.Count)];
                    while (parent2Idx == parent1Idx);
                    nn.CrossOver(network, agents[parent1Idx].nnOffset, agents[parent2Idx].nnOffset, agents[childIdx].nnOffset, rnd);
                }
            }
        }
    }
}
