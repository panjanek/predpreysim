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
                    Name = "blue fitness",
                    Selector = s=>s.topBlueAvgFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Blue }
                },
                new StatsSeries() {
                    Name = "blue meals",
                    Selector = s=>s.topBlueAvgMeals,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Blue, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round }
                },
                new StatsSeries() {
                    Name = "blue deaths",
                    Selector = s=>s.topBlueAvgDeaths,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Blue, StrokeDashArray = new DoubleCollection() { 3, 6 } }
                },
                new StatsSeries() {
                    Name = "red fitness",
                    Selector = s=>s.topRedAvgFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Red }
                },
                new StatsSeries() {
                    Name = "red meals",
                    Selector = s=>s.topRedAvgMeals,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Red, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round  }
                },
            };

            Loaded += ConfigWindow_Loaded;
        }

        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            seriesCollection = new ObservableCollection<SeriesOptionItem>();
            foreach (var serie in series)
            {
                seriesCollection.Add(new SeriesOptionItem() { IsSelected = true, Name = serie.Name, Series = serie });
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
            if (seriesCollection == null || seriesCollection.Count(c => c.IsSelected) == 0)
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
