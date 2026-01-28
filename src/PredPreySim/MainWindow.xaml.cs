using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AppContext = PredPreySim.Models.AppContext;
using Application = System.Windows.Application;

namespace PredPreySim
{
    //TODO:
    // - improve graphs
    // - select and track
    // - show best performers
    // - age contribute to fitness!
    // - memory
    public partial class MainWindow : Window
    {
        private bool uiPending;

        private DateTime lastCheckTime;

        private long lastCheckFrameCount;

        private AppContext app;
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void parent_Loaded(object sender, RoutedEventArgs e)
        {
            app = new AppContext(this);
            KeyDown += MainWindow_KeyDown;
            System.Timers.Timer systemTimer = new System.Timers.Timer() { Interval = 10 };
            systemTimer.Elapsed += SystemTimer_Elapsed;
            systemTimer.Start();
            DispatcherTimer infoTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0) };
            infoTimer.Tick += InfoTimer_Tick;
            infoTimer.Start();
        }

        public void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    app.renderer.Paused = !app.renderer.Paused;
                    e.Handled = true;
                    break;
            }
        }

        private void SystemTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!uiPending)
            {
                uiPending = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        app.renderer.Step();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        uiPending = false;
                    }

                    uiPending = false;
                }), DispatcherPriority.Render);
            }
        }

        private void InfoTimer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var timespan = now - lastCheckTime;
            double frames = app.renderer.FrameCounter - lastCheckFrameCount;
            if (timespan.TotalSeconds >= 0.0001)
            {
                var bestBlue = app.simulation.stats.Count == 0 ? 0 : 100 * app.simulation.stats.Max(s => s.topBlueMealsPerAge);
                var bestRed = app.simulation.stats.Count == 0 ? 0 : 100 * app.simulation.stats.Max(s => s.topRedMealsPerAge);

                double fps = frames / timespan.TotalSeconds;
                Title = $"Predator Pray Sim. " +
                        $"fps:{fps.ToString("0.0")} " +
                        $"step:{app.simulation.step} " +
                        $"gen:{app.simulation.generation} " +
                        $"blue: {bestBlue.ToString("0.00")} " +
                        $"red: {bestRed.ToString("0.00")} ";

                lastCheckFrameCount = app.renderer.FrameCounter;
                lastCheckTime = now;
            }
        }
    }
}