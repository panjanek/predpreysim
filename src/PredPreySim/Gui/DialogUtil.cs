using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using AppContext = PredPreySim.Models.AppContext;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;

namespace PredPreySim.Gui
{
    public static class DialogUtil
    {
        public static StartNewSimulationParameters ShowStartNewSimulationDialog()
        {
            // Window
            Window dialog = new Window()
            {
                Width = 400,
                Height = 250,
                Title = "Start new simulation",
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.ToolWindow,
                Owner = Application.Current.MainWindow
            };

            // Layout
            StackPanel panel = new StackPanel() { Margin = new Thickness(10) };

            TextBlock txt = new TextBlock() { Text = "Choose simulation start parameters:", Margin = new Thickness(0, 0, 0, 10) };

            ComboBox sizeCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "960x540", Tag = new StartNewSimulationParameters() { width = 960, height = 540 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "1280x1080", Tag = new StartNewSimulationParameters() { width = 1280, height = 720 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "1920x1080", Tag = new StartNewSimulationParameters() { width = 1920, height = 1080 }, IsSelected = true });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "2880x1620", Tag = new StartNewSimulationParameters() { width = 2880, height = 1620 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "3840x2160", Tag = new StartNewSimulationParameters() { width = 3840, height = 2160 } });
            sizeCombo.Items.Add(new ComboBoxItem() { Content = "7680x4320", Tag = new StartNewSimulationParameters() { width = 7680, height = 4320 } });

            ComboBox countCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            countCombo.Items.Add(new ComboBoxItem() { Content = "1000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 1000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "2000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 2000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "3000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 3000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "5000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 5000 }, IsSelected = true });
            countCombo.Items.Add(new ComboBoxItem() { Content = "8000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 8000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "10000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 10000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "15000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 15000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "20000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 20000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "30000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 30000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "40000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 40000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "50000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 50000 } });
            countCombo.Items.Add(new ComboBoxItem() { Content = "100000 agents", Tag = new StartNewSimulationParameters() { agentsCount = 100000 } });

            ComboBox proportionCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            proportionCombo.Items.Add(new ComboBoxItem() { Content = "60% plants / 40% prey / 10% predators", Tag = new StartNewSimulationParameters() { plantsRatio = 0.6, predatorsRatio = 0.1 }, IsSelected = true });
            proportionCombo.Items.Add(new ComboBoxItem() { Content = "50% plants / 25% prey / 25% predators", Tag = new StartNewSimulationParameters() { plantsRatio = 0.5, predatorsRatio = 0.25 } });
            proportionCombo.Items.Add(new ComboBoxItem() { Content = "70% plants / 25% prey / 5% predators", Tag = new StartNewSimulationParameters() { plantsRatio = 0.7, predatorsRatio = 0.05 } });
            proportionCombo.Items.Add(new ComboBoxItem() { Content = "50% plants / 45% prey / 5% predators", Tag = new StartNewSimulationParameters() { plantsRatio = 0.5, predatorsRatio = 0.05 } });
            proportionCombo.Items.Add(new ComboBoxItem() { Content = "30% plants / 40% prey / 30% predators", Tag = new StartNewSimulationParameters() { plantsRatio = 0.3, predatorsRatio = 0.4 } });

            ComboBox seedCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Initialize agents randomly with fixed seed", Tag = new StartNewSimulationParameters() { fixedSeed = true }, IsSelected = true });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Initialize agents randomly with random seed", Tag = new StartNewSimulationParameters() { fixedSeed = false } });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Use agents from current simulation", Tag = new StartNewSimulationParameters() { useExistingAgents = true } });

            // Buttons
            StackPanel buttonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Button ok = new Button() { Content = "Start!", Width = 70, Margin = new Thickness(5, 0, 0, 0) };
            Button cancel = new Button() { Content = "Cancel", Width = 70 };

            ok.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
            cancel.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(cancel);
            buttonPanel.Children.Add(ok);

            // Compose UI
            panel.Children.Add(txt);
            panel.Children.Add(sizeCombo);
            panel.Children.Add(countCombo);
            panel.Children.Add(proportionCombo);
            panel.Children.Add(seedCombo);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            // Show input dialog
            if (dialog.ShowDialog() == true)
            {
                var selectedSize = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(sizeCombo.SelectedItem);
                var selectedCount = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(countCombo.SelectedItem);
                var selectedPoportion = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(proportionCombo.SelectedItem);
                var selectedSeed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(seedCombo.SelectedItem);
                return new StartNewSimulationParameters()
                {
                    agentsCount = selectedCount.agentsCount,
                    width = selectedSize.width,
                    height = selectedSize.height,
                    plantsRatio = selectedPoportion.plantsRatio,
                    predatorsRatio = selectedPoportion.predatorsRatio,
                    fixedSeed = selectedSeed.fixedSeed,
                    useExistingAgents = selectedSeed.useExistingAgents
                };
            }
            else
            {
                return null;
            }
        }
    }

    public class StartNewSimulationParameters
    {
        public int width;

        public int height;

        public double plantsRatio;

        public double predatorsRatio;

        public int agentsCount;

        public bool fixedSeed;

        public bool useExistingAgents;
    }
}
