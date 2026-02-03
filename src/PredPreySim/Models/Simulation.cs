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
using PredPreySim.Gui;
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

        public NetworkConfig networkConfig = new NetworkConfig() { inputs = 19, hidden = 12, outputs = 4, memoryInputs = [17, 18], memoryOutputs = [2, 3] };

        [JsonIgnore]
        public INeuralNetwork nn;

        [JsonIgnore]
        private Random rnd = new Random(1);

        [JsonIgnore]
        public Func<INeuralNetwork, float[], int, int, double> diversityNorm = DistanceMatrix.BehavioralDistance;

        public List<int> topBlueIds = new List<int>();

        public List<int> topRedIds = new List<int>();

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

        public Simulation(StartNewSimulationParameters parameters)
        {
            InitWithParameters(parameters);
        }

        private void InitWithParameters(StartNewSimulationParameters parameters)
        {
            shaderConfig = new ShaderConfig();
            shaderConfig.width = parameters.width;
            shaderConfig.height = parameters.height;
            shaderConfig.agentsCount = parameters.agentsCount;
            agents = new Agent[shaderConfig.agentsCount];
            nn = new NeuralNetwork(networkConfig);
            InitRandomly(parameters.plantsRatio, parameters.predatorsRatio);
            rnd = parameters.fixedSeed ? new Random(1) : new Random();
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
                                        + agent.meals * 4 //was 2
                                        + Math.Sqrt(agent.survivalDuration) * 0.2 //was: agent.survivalDuration * 0.003
                                        - agent.deaths * 3 //was 5
                                        - agent.energySpent * 0.003 //was 0.002, then 0.01
                                    : // predator
                                        + agent.meals * 3 //10?
                                        + agent.nearPrey * 0.015 //was 0.005?
                                        + 0.1 * shaderConfig.generationDuration * agent.meals / (agent.age + 1.0) //agent.meals * Math.Exp(-agent.age / shaderConfig.generationDuration)
                                        - agent.energySpent * 0.005; // was 0.001, then 0.02
        }

        public double GetFitness(Agent agent)
        {
            var raw = GetRawFitness(agent);
            return raw * Math.Exp(-agent.age / (2.0 * shaderConfig.generationDuration));
        }

        private (List<int>, List<int>) Selection(List<RankedAgent> ranking, int type, double candidateRatio, double selectRatio, double bottomRatio, int diversitySoftening)
        {
            var all = ranking.Where(x => x.agent.type == type);
            var allCount = all.Count();
            int candidateCount = (int)Math.Ceiling(allCount * candidateRatio);      // first phase: select this many of best agents
            int selectCount = (int)Math.Ceiling(allCount * selectRatio);            // then select subset of diverse agents amont them - these will breed
            int bottomCount = (int)Math.Ceiling(allCount * bottomRatio);            // this many worsst performers will be replaced

            // select candidates
            var candidates = all.OrderByDescending(x => x.fitness).Take(candidateCount).ToList();
            var distanceMatrix = new DistanceMatrix(this, candidates.Select(x => x.index).ToList());
            List<RankedAgent> selected = [candidates[0]];
            candidates.Remove(candidates[0]);
            while (selected.Count < selectCount) // greedy algorithm for selecting diverse subset
            {
                var currentIndexes = selected.Select(r => r.index).ToList();
                var candidatesByDiversity = candidates.Select(t => new RankedAgentWithDistance() { ranked = t, distance = distanceMatrix.GetMinDistance(t.index, currentIndexes) });
                var bestCandidates = candidatesByDiversity.OrderByDescending(c => c.distance).Take(diversitySoftening);
                var select = bestCandidates.OrderByDescending(c => c.ranked.fitness).First();
                selected.Add(select.ranked);
                candidates.Remove(select.ranked);
            }

            var selectedIds = selected.OrderByDescending(r=>r.fitness).Select(x => x.index).ToList(); //important: ordered from best to worst
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

            // select and breed blue
            (var selectedBlueIds, var bottomBlueIds) = Selection(ranking, 1, 0.2, 0.1, 0.5, 3);
            var selectedBlueDistanceMatrix = new DistanceMatrix(this, selectedBlueIds);
            Breed(selectedBlueIds, bottomBlueIds, selectedBlueDistanceMatrix);

            //select and breed red
            (var selectedRedIds, var bottomRedIds) = Selection(ranking, 2, 0.2, 0.1, 0.5, 3);
            var selectedRedDistanceMatrix = new DistanceMatrix(this, selectedRedIds);
            Breed(selectedRedIds, bottomRedIds, selectedRedDistanceMatrix);

            // record stats
            topBlueIds = selectedBlueIds;
            topRedIds = selectedRedIds;
            stats.Add(new Stats(this, ranking, selectedBlueIds, selectedRedIds, selectedBlueDistanceMatrix, selectedRedDistanceMatrix));

            // set visual flags
            SetFlags();
        }

        public void SetFlags()
        {
            if (topBlueIds != null && topRedIds != null)
            {
                var bestBlueIdx = topBlueIds.Count > 0 ? topBlueIds[0] : -1;
                var bestRedIdx = topRedIds.Count > 0 ? topRedIds[0] : -1;
                var topBlueSet = new HashSet<int>(topBlueIds);
                var topRedSet = new HashSet<int>(topRedIds);
                for (int i = 0; i < agents.Length; i++)
                {
                    int flag = 1;

                    if (topBlueSet.Contains(i) || topRedSet.Contains(i))
                        flag = 2;

                    if (i == bestBlueIdx || i == bestRedIdx)
                        flag = 3;

                    agents[i].flag = flag;
                }
            }
        }

        private int Pick(List<int> indexes, double alpha, double beta, int excludeIndex = -1)
        {
            int pickedPos;
            var distributionDecision = rnd.NextDouble();
            do
            {
                if (distributionDecision < 0.15)          // 15% of times pick with uniform distribution
                    pickedPos = rnd.Next(indexes.Count);
                else                                      // 85% of times pick with beta distribution (top performers more often)
                {
                    var x = MathUtil.NextBeta(rnd, alpha, beta);
                    pickedPos = Math.Min((int)(x * indexes.Count), indexes.Count - 1);
                }
            }
            while (indexes[pickedPos] == excludeIndex);
            return indexes[pickedPos];
        }

        // Take agents from "parents" indexes in agents array and breed. Overwrite agents from "spaces" indexes with newly created. 
        private void Breed(List<int> parents, List<int> spaces, DistanceMatrix distanceMatrix)
        {
            foreach(var childIdx in spaces)
            {
                var parent1Idx = Pick(parents, 0.7, 2);

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
                else if (decision1 < 0.80) // 65% of times: mutate, single parent
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
                else // 20% of times: cross-over, two parents
                {
                    //var parent2Idx = Pick(parents, 0.7, 2, parent1Idx);

                    var parentsByDistance = parents.OrderByDescending(p => distanceMatrix.GetDistance(parent1Idx, p)).ToList();
                    var parent2Idx = Pick(parentsByDistance, 0.8, 1.5, parent1Idx);

                    nn.CrossOver(network, agents[parent1Idx].nnOffset, agents[parent2Idx].nnOffset, agents[childIdx].nnOffset, rnd);
                    if (rnd.NextDouble() < 0.6) // 60% of the time apply weak mutation to the child
                    {
                        double probabilityAmplification = 0.3 * (1.0 + 4 * rnd.NextDouble());
                        double magnitudeAmplification = 0.3 * (1.0 + 4 * rnd.NextDouble());
                        nn.Mutate(network, agents[childIdx].nnOffset, rnd, 0.01 * probabilityAmplification, 0.05 * magnitudeAmplification);
                    }
                }
            }
        }
    }
}
