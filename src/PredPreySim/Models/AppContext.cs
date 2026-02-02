using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PredPreySim.Gpu;
using PredPreySim.Gui;
using PredPreySim.Utils;

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
            this.simulation = new Simulation();
            simulation.SetFlags();
            renderer = new OpenGlRenderer(mainWindow.placeholder, this);
            renderer.UploadAgents();
            renderer.ResetOrigin();
            configWindow = new ConfigWindow(this);
            configWindow.Show();
            configWindow.Activate();
        }

        public void Load(string fn)
        {
            string json = File.ReadAllText(fn);
            simulation = SerializationUtil.DeserializeFromJson(json);
            simulation.InitAfterLoad();
            renderer.UploadAgents();
            renderer.ClearTextures();
            configWindow.DrawStats(simulation.stats);
        }

        public void Save(string fn)
        {
            renderer.DownloadAgents();
            var json = SerializationUtil.SerializeToJson(simulation);
            File.WriteAllText(fn, json);
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
