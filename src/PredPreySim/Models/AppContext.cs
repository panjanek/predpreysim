using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PredPreySim.Gpu;
using PredPreySim.Gui;

namespace PredPreySim.Models
{
    public class AppContext
    {
        public Simulation simulation;

        public MainWindow mainWindow;

        public OpenGlRenderer renderer;

        public ConfigWindow configWindow;

        public AppContext(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            simulation = new Simulation();
            renderer = new OpenGlRenderer(mainWindow.placeholder, this);
            renderer.UploadAgents();
            renderer.ResetOrigin();
            configWindow = new ConfigWindow(this);
            configWindow.Show();
            configWindow.Activate();
        }

        public void DrawStats()
        {
            if (simulation.stats.Count > 2)
            {
                WpfUtil.DispatchRender(configWindow.Dispatcher, () => configWindow.DrawStats(simulation.stats));
            }
        }
    }
}
