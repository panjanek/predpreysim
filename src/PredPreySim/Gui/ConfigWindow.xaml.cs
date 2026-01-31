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

        private ObservableCollection<StatsSeries> seriesCollection;
        public ConfigWindow(AppContext app)
        {
            this.app = app;
            InitializeComponent();
            customTitleBar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
            minimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;
            Closing += (s, e) => { e.Cancel = true; WindowState = WindowState.Minimized; };
            seriesCollection = new ObservableCollection<StatsSeries>()
            {
                // -------------------------------------------- AVG -------------------------------------------
                new StatsSeries() {
                    Name = "blue avg fitness",
                    Selector = s=>s.topBlueAvgFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Blue },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "blue avg meals",
                    Selector = s=>s.topBlueAvgMeals,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Blue, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "blue avg deaths",
                    Selector = s=>s.topBlueAvgDeaths,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Blue, StrokeDashArray = new DoubleCollection() { 3, 6 } },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red avg fitness",
                    Selector = s=>s.topRedAvgFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Red },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red avg meals",
                    Selector = s=>s.topRedAvgMeals,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Red, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round  },
                    IsSelected = false
                },

                // -------------------------------------------------- MEDIAN ------------------------------------
                new StatsSeries() {
                    Name = "blue med fitness",
                    Selector = s=>s.topBlueMedFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Blue },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "blue med meals",
                    Selector = s=>s.topBlueMedMeals,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Blue, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "blue med deaths",
                    Selector = s=>s.topBlueMedDeaths,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Blue, StrokeDashArray = new DoubleCollection() { 3, 6 } },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red med fitness",
                    Selector = s=>s.topRedMedFitness,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Red },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red med meals",
                    Selector = s=>s.topRedMedMeals,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Red, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round  },
                    IsSelected = false
                },

                // -------------------------------------------------- OTHER ------------------------------------
                new StatsSeries() {
                    Name = "blue avg age",
                    Selector = s=>s.topBlueAvgAge,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Cyan },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "red avg age",
                    Selector = s=>s.topRedAvgAge,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Magenta },
                    IsSelected = false
                },
                new StatsSeries() {
                    Name = "blue meals/age",
                    Selector = s=>s.topBlueMealsPerAge,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Cyan },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "red meals/age",
                    Selector = s=>s.topRedMealsPerAge,
                    Style = new SeriesStyle() { StrokeThickness = 3, Stroke = Brushes.Magenta },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "active plants",
                    Selector = s=>s.plantsCount,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Green },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "blue deaths",
                    Selector = s=>s.blueDeaths,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Yellow },
                    IsSelected = true
                },

                new StatsSeries() {
                    Name = "top near prey",
                    Selector = s=>s.topNearPrey,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Gray },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "all near prey",
                    Selector = s=>s.allNearPrey,
                    Style = new SeriesStyle() { StrokeThickness = 2, Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection() { 0, 3 }, LineCap = PenLineCap.Round },
                    IsSelected = true
                },

                new StatsSeries() {
                    Name = "blue energy",
                    Selector = s=>s.topBlueEnergySpent,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Blue },
                    IsSelected = true
                },
                new StatsSeries() {
                    Name = "red energy",
                    Selector = s=>s.topRedEnergySpent,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Red },
                    IsSelected = true
                },

                new StatsSeries() {
                    Name = "age2",
                    Selector = s=>s.topRedEnergySpent,
                    Style = new SeriesStyle() { StrokeThickness = 1, Stroke = Brushes.Pink },
                    IsSelected = true
                },
            };

            Loaded += ConfigWindow_Loaded;
        }

        private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
                return seriesCollection.Where(c => c.IsSelected).Select(c => c).ToList();

        }

        private void SeriesCheckBox_Click(object sender, RoutedEventArgs e) => statsGraph.UpdateSeries(GetVisibleSeries());
    }
}
