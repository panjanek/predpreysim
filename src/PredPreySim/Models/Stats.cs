using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredPreySim.Utils;

namespace PredPreySim.Models
{
    public class Stats
    {
        public Stats() { }

        public Stats(Simulation sim, List<RankedAgent> ranking, List<int> selectedBlueIds, List<int> selectedRedIds)
        {
            var allBlueCount = ranking.Count(a => a.agent.type == 1);
            var allBlue = ranking.Where(x => x.agent.type == 1);
            var topBlue = allBlue.OrderByDescending(x => x.fitness).Take(allBlueCount / 10).ToList();

            var allRedCount = ranking.Count(a => a.agent.type == 2);
            var allRed = ranking.Where(x => x.agent.type == 2);
            var topRed = allRed.OrderByDescending(x => x.fitness).Take(allRedCount / 10).ToList();

            time = sim.shaderConfig.t;
            topBlueAvgFitness = topBlue.Average(x => sim.GetRawFitness(x.agent));
            topRedAvgFitness = topRed.Average(x => sim.GetRawFitness(x.agent));

            topBlueMedFitness = topBlue.Median(x => sim.GetRawFitness(x.agent));
            topRedMedFitness = topRed.Median(x => sim.GetRawFitness(x.agent));

            topBlueMealsPerAge = topBlue.Average(x => x.agent.age == 0 ? 0 : 1.0 * x.agent.meals / x.agent.age);
            topRedMealsPerAge = topRed.Average(x => x.agent.age == 0 ? 0 : 1.0 * x.agent.meals / x.agent.age);

            topBlueAvgAge = topBlue.Average(x => x.agent.age * 1.0);
            topRedAvgAge = topRed.Average(x => x.agent.age * 1.0);

            plantsCount = ranking.Where(a => a.agent.type == 0 && a.agent.state == 0).Count();
            blueDeaths = allBlue.Sum(a => a.agent.deaths * 1.0) / allBlue.Count();

            topNearPrey = topRed.Average(x => x.agent.nearPrey);
            allNearPrey = allRed.Average(x => x.agent.nearPrey);

            topBlueEnergySpent = topBlue.Average(x => x.agent.energySpent);
            topRedEnergySpent = topRed.Average(x => x.agent.energySpent);

            topSurvival = topBlue.Average(x => x.agent.survivalDuration);

            var blueMatrixL2 = new DistanceMatrix(sim, selectedBlueIds, DistanceMatrix.L2Distance);
            var redMatrixL2 = new DistanceMatrix(sim, selectedRedIds, DistanceMatrix.L2Distance);
            var blueMatrixBevavioral = new DistanceMatrix(sim, selectedBlueIds, DistanceMatrix.BehavioralDistance);
            var redMatrixBevavioral = new DistanceMatrix(sim, selectedRedIds, DistanceMatrix.BehavioralDistance);
            blueDiversityL2 = blueMatrixL2.GetDiversity();
            redDiversityL2 = redMatrixL2.GetDiversity();
            blueDiversityBehavioral = blueMatrixBevavioral.GetDiversity();
            redDiversityBehavioral = redMatrixBevavioral.GetDiversity();
        }




        public double time;

        public double topBlueAvgFitness;

        public double topRedAvgFitness;

        public double topBlueMedFitness;

        public double topRedMedFitness;

        public double topRedAvgAge;

        public double topBlueAvgAge;

        public double topRedMealsPerAge;

        public double topBlueMealsPerAge;

        public double plantsCount;

        public double blueDeaths;

        public double topNearPrey;

        public double allNearPrey;

        public double topBlueEnergySpent;

        public double topRedEnergySpent;

        public double topSurvival;

        public double blueDiversityL2;

        public double blueDiversityBehavioral;

        public double redDiversityL2;

        public double redDiversityBehavioral;
    }
}
