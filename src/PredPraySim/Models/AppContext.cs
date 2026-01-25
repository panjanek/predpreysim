using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredPraySim.Gpu;

namespace PredPraySim.Models
{
    public class AppContext
    {
        public Simulation simulation;

        public MainWindow mainWindow;

        public OpenGlRenderer renderer;

        public AppContext(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            simulation = new Simulation();
            renderer = new OpenGlRenderer(mainWindow.placeholder, this);
            renderer.UploadAgents(simulation.agents);
            renderer.ResetOrigin();
        }
    }
}
