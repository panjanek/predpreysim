using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private List<StatsSeries> series;

        private ObservableCollection<SeriesOptionItem> seriesCollection;
        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            series = new List<StatsSeries>()
            {
                new StatsSeries() {
                    name = "blue fitness",
                    line = Brushes.Blue,
                    thickness = 2,
                    dot = Brushes.Blue,
                    radius = 5,
                    selector = s=>s.topBlueAvgFitness
                },
                new StatsSeries() {
                    name = "blue meals",
                    line = Brushes.Blue,
                    style = LineStyle.Dotted,
                    thickness = 2,
                    dot = Brushes.Blue,
                    radius = 3,
                    selector = s=>s.topBlueAvgMeals
                },
                new StatsSeries() {
                    name = "blue deaths",
                    line = Brushes.Blue,
                    style = LineStyle.Dashed,
                    thickness = 1,
                    dot = Brushes.Blue,
                    radius = 3,
                    selector = s=>s.topBlueAvgDeaths
                },
                new StatsSeries() {
                    name = "red fitness",
                    line = Brushes.Red,
                    thickness = 2,
                    dot = Brushes.Red,
                    radius = 3,
                    selector = s=>s.topRedAvgFitness
                },
                new StatsSeries() {
                    name = "red meals",
                    line = Brushes.Red,
                    style = LineStyle.Dotted,
                    thickness = 2,
                    dot = Brushes.Red,
                    radius = 3,
                    selector = s=>s.topRedAvgMeals
                },
            };

            Loaded += ConfigWindow_Loaded;
        }

        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            seriesCollection = new ObservableCollection<SeriesOptionItem>();
            foreach (var serie in series)
            {
                seriesCollection.Add(new SeriesOptionItem() { IsSelected = true, Name = serie.name, Series = serie });
            }

            seriesList.ItemsSource = seriesCollection;
        }

        public void DrawStats(List<Stats> stats)
        {
            statsGraph.UpdateSeries(GetVisibleSeries());
            statsGraph.Draw(stats);
        }

        public List<StatsSeries> GetVisibleSeries()
        {
            if (seriesCollection == null || seriesCollection.Count == 0)
                return series;
            else
                return seriesCollection.Where(c => c.IsSelected).Select(c => c.Series).ToList();

        }

        private void SeriesCheckBox_Click(object sender, RoutedEventArgs e) => statsGraph.UpdateSeries(GetVisibleSeries());
    }

    public class SeriesOptionItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public StatsSeries Series { get; set; }
    }
}
