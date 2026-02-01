using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        public float[] kernelRed;

        public float[] kernelGreen;

        public float[] kernelBlue;

        public float decayRed = 0.99f;

        public float decayGreen = 0.99f;

        public float decayBlue = 0.994f;

        public int step;

        public int generation;

        public List<Stats> stats;

        public NetworkConfig networkConfig = new NetworkConfig() { inputs = 19, hidden = 8, outputs = 4, memoryInputs = [17, 18], memoryOutputs = [2, 3] };

        [JsonIgnore]
        public INeuralNetwork nn;

        [JsonIgnore]
        private Random rnd = new Random(1);

        [JsonIgnore]
        public Func<INeuralNetwork, float[], int, int, double> diversityNorm = DistanceMatrix.L2Distance;

        public Simulation()
        {
            shaderConfig = new ShaderConfig();
            agents = new Agent[shaderConfig.agentsCount];
            nn = new NeuralNetwork(networkConfig);
            InitRandomly(0.6, 0.1);
            kernelRed = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayRed);
            kernelGreen = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayGreen);
            kernelBlue = MathUtil.Normalize(Blurs.AvailableKernels["Strong"], decayBlue);
            stats = new List<Stats>();
        }

        public void InitAfterLoad()
        {
            nn = new NeuralNetwork(networkConfig);
        }

        private void InitRandomly(double plants, double predators)
        {
            int networksCount = 0;
            for (int i = 0; i < agents.Length; i++)
            {
                var r = rnd.NextDouble();

                agents[i] = new Agent();
                agents[i].type = r < plants ? 0 : (r < (1 - predators) ? 1 : 2);
                agents[i].energy = shaderConfig.initialEnergy;
                agents[i].angle = (float)(2 * Math.PI * rnd.NextDouble());
                agents[i].SetPosition(new Vector2((float)(shaderConfig.width * rnd.NextDouble()), (float)(shaderConfig.height * rnd.NextDouble())));

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

        public double GetRawFitness(Agent agent)
        {
            return agent.type == 1  ? // prey
                                        + agent.meals * 2
                                        + agent.survivalDuration * 0.003 // was 0.002
                                        - agent.deaths * 5
                                        - agent.energySpent * 0.002 // was 0.001
                                    : // predator
                                        + agent.meals * 3 //10?
                                        + agent.nearPrey * 0.005 //0.01?
                                        - agent.energySpent * 0.002; // was 0.001
        }

        public double GetFitness(Agent agent)
        {
            var raw = GetRawFitness(agent);
            return raw * Math.Exp(-agent.age / (2.0 * shaderConfig.generationDuration));
        }

        private (List<int>, List<int>) Selection(List<RankedAgent> ranking, int type)
        {
            var all = ranking.Where(x => x.agent.type == type);
            var allCount = all.Count();
            int topCount = allCount / 4;      // first phase: select this many of best agents
            int selectCount = allCount / 10;  // then select subset of diverse agents amont them - these will breed
            int bottomCount = allCount / 2;   // this many worse performers will be replaced

            var top = all.OrderByDescending(x => x.fitness).Take(topCount).ToList();
            var distanceMatrix = new DistanceMatrix(this, top.Select(x => x.index).ToList());
            List<RankedAgent> selected = new List<RankedAgent>();
            selected.Add(top[0]);
            top.Remove(top[0]);
            while (selected.Count < selectCount)
            {
                var currentIndexes = selected.Select(r => r.index).ToList();
                var candidates = top.Select(t => new RankedAgentWithDistance() { ranked = t, distance = distanceMatrix.GetMinDistance(t.index, currentIndexes) });
                var bestCandidates = candidates.OrderByDescending(c => c.distance).Take(3);
                var select = bestCandidates.OrderByDescending(c => c.ranked.fitness).First();
                selected.Add(select.ranked);
                top.Remove(select.ranked);
            }

            //selected = all.OrderByDescending(x => x.fitness).Take(selectCount).ToList(); //these will breed
            var selectedIds = selected.Select(x => x.index).ToList();
            var bottom = all.OrderBy(x => x.fitness).Take(bottomCount).ToList(); //this will be replaced with newly created agents
            var bottomIds = bottom.Select(x => x.index).ToList(); 
            if (selectedIds.Intersect(bottomIds).Count() > 0)
                throw new Exception("!");

            return (selectedIds, bottomIds);
        }

        public void ChangeEpoch()
        {
            generation++;
            var ranking = agents.Select((a, i) => new RankedAgent() { index = i, agent = a, fitness = GetFitness(a) }).Where(a=>a.agent.type > 0).ToList();

            (var selectedBlueIds, var bottomBlueIds) = Selection(ranking, 1);
            Breed(selectedBlueIds, bottomBlueIds);

            (var selectedRedIds, var bottomRedIds) = Selection(ranking, 2);
            Breed(selectedRedIds, bottomRedIds);

            // record stats
            stats.Add(new Stats(this, ranking, selectedBlueIds, selectedRedIds));

            //highlight best
            var bestBlueIdx = selectedBlueIds[0];
            var bestRedIdx = selectedRedIds[0];
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
                agents[childIdx].survivalDuration = 0;
                agents[childIdx].meals = 0;
                agents[childIdx].deaths = 0;
                agents[childIdx].energySpent = 0;
                agents[childIdx].energy = shaderConfig.initialEnergy;
                agents[childIdx].memory0 = 0;
                agents[childIdx].memory1 = 0;
                agents[childIdx].nearPrey = 0;
                agents[childIdx].SetPosition(agents[parent1Idx].position + new Vector2((float)rnd.NextDouble() * 10 - 5, (float)rnd.NextDouble() * 10 - 5));
                agents[childIdx].angle = (float)(2 * Math.PI * rnd.NextDouble());

                var decision1 = rnd.NextDouble();
                if (decision1 < 0.15) // 15% of times: copy without changing
                {
                    Array.Copy(network, agents[parent1Idx].nnOffset, network, agents[childIdx].nnOffset, nn.Size);
                }
                else if (decision1 < 0.9) // 75% of times: mutate, single parent
                {
                    Array.Copy(network, agents[parent1Idx].nnOffset, network, agents[childIdx].nnOffset, nn.Size);

                    double probabilityAmplification = 1.0 + 4 * rnd.NextDouble();
                    double magnitudeAmplification = 1.0 + 4 * rnd.NextDouble(); 

                    double decision2 = rnd.NextDouble();
                    if (decision2 < 0.6) //60% - mutate slightly
                        nn.Mutate(network, agents[childIdx].nnOffset, rnd, 0.01 * probabilityAmplification, 0.05 * magnitudeAmplification);
                    else if (decision2 < 0.95) //35% - mutate mildly
                        nn.Mutate(network, agents[childIdx].nnOffset, rnd, 0.05 * probabilityAmplification, 0.15 * magnitudeAmplification);
                    else //5% - mutate strong all inputs of one hidden neuron
                        nn.MutateAllIncomming(network, agents[childIdx].nnOffset, rnd, 0.3 * magnitudeAmplification);
                }
                else // 10% of times: cross-over, two parents
                {
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
