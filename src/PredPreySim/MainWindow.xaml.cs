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
using PredPreySim.Gui;
using PredPreySim.Utils;
using AppContext = PredPreySim.Models.AppContext;
using Application = System.Windows.Application;

namespace PredPreySim
{
    //TODO:
    // - rescale absolute sensors to -0.5,0.5 or -1,1 (???)
    // - longer tail
    // - tune speeds
    // - tune blurring (prey max?)
    public partial class MainWindow : Window
    {
        private bool uiPending;

        private DateTime lastCheckTime;

        private long lastCheckFrameCount;

        private FullscreenWindow fullscreen;

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
            System.Timers.Timer systemTimer = new System.Timers.Timer() { Interval = 5 };
            systemTimer.Elapsed += SystemTimer_Elapsed;
            systemTimer.Start();
            DispatcherTimer infoTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0) };
            infoTimer.Tick += InfoTimer_Tick;
            infoTimer.Start();
            DebugUtil.TestBeta();
        }

        public void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    app.renderer.Paused = !app.renderer.Paused;
                    e.Handled = true;
                    break;
                case Key.F:
                    ToggleFullscreen();
                    e.Handled = true;
                    break;
            }
        }

        public void ToggleFullscreen()
        {
            if (fullscreen == null)
            {
                parent.Children.Remove(placeholder);
                fullscreen = new FullscreenWindow() { Owner = Window.GetWindow(this) };
                fullscreen.KeyDown += MainWindow_KeyDown;
                fullscreen.ContentHost.Content = placeholder;
                fullscreen.Show();
            }
            else
            {
                fullscreen.ContentHost.Content = null;
                parent.Children.Add(placeholder);
                fullscreen.Close();
                fullscreen = null;
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
                var blueMealsPerAge = app.simulation.stats.Count == 0 ? 0 : 100 * app.simulation.stats.Max(s => s.topBlueMealsPerAge);
                var redMealsPerAge = app.simulation.stats.Count == 0 ? 0 : 100 * app.simulation.stats.Max(s => s.topRedMealsPerAge);

                var blueFitness = app.simulation.stats.Count == 0 ? 0 : 100 * app.simulation.stats.Max(s => s.topBlueMedFitness);
                var redFitness = app.simulation.stats.Count == 0 ? 0 : 100 * app.simulation.stats.Max(s => s.topRedMedFitness);

                double fps = frames / timespan.TotalSeconds;
                Title = $"Predator Prey Sim. " +
                        $"fps:{fps.ToString("0.0")} " +
                        $"step:{app.simulation.step} " +
                        $"gen:{app.simulation.generation} " +
                        $"blue meals: {blueMealsPerAge.ToString("0.000")} " +
                        $"red meals: {redMealsPerAge.ToString("0.000")} " +
                        $"blue fitness: {blueFitness.ToString("0.0")} " +
                        $"red fitness: {redFitness.ToString("0.0")} ";

                lastCheckFrameCount = app.renderer.FrameCounter;
                lastCheckTime = now;
            }
        }
    }
}