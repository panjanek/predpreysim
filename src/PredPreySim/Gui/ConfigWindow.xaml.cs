using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PredPreySim.Models;
using AppContext = PredPreySim.Models.AppContext;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace PredPreySim.Gui
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private AppContext app;
        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
        }

        public void DrawStats(List<Stats> stats)
        {
            statsGraph.Draw(stats, new List<StatsSeries>()
            {
                new StatsSeries() { 
                    name = "blue fitness",
                    line = Brushes.Blue, 
                    thickness = 2,
                    dot = Brushes.Blue,
                    radius = 5,
                    selector = s=>s.topBlueFitness 
                },
                new StatsSeries() {
                    name = "blue meals",
                    line = Brushes.Blue,
                    style = LineStyle.Dotted,
                    thickness = 2,
                    dot = Brushes.Blue,
                    radius = 3,
                    selector = s=>s.topBlueMeals
                },
                new StatsSeries() {
                    name = "blue deaths",
                    line = Brushes.Blue,
                    style = LineStyle.Dashed,
                    thickness = 1,
                    dot = Brushes.Blue,
                    radius = 3,
                    selector = s=>s.topBlueDeaths
                },
                new StatsSeries() {
                    name = "red fitness",
                    line = Brushes.Red,
                    thickness = 2,
                    dot = Brushes.Red,
                    radius = 3,
                    selector = s=>s.topRedFitness
                },
                new StatsSeries() {
                    name = "red meals",
                    line = Brushes.Red,
                    style = LineStyle.Dotted,
                    thickness = 2,
                    dot = Brushes.Red,
                    radius = 3,
                    selector = s=>s.topRedMeals
                },
            });
        }
    }
}
