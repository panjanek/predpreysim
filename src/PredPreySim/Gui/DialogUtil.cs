using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using PredPreySim.Models;
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
                Height = 470,
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

            ComboBox decayGreenCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            decayGreenCombo.Items.Add(new ComboBoxItem() { Content = "Shortest plant scent radius (decay = 0.950)", Tag = new StartNewSimulationParameters() { decayGreen = 0.950f } });
            decayGreenCombo.Items.Add(new ComboBoxItem() { Content = "Shorter plant scent radius (decay = 0.970)", Tag = new StartNewSimulationParameters() { decayGreen = 0.970f } });
            decayGreenCombo.Items.Add(new ComboBoxItem() { Content = "Short plant scent radius (decay = 0.980)", Tag = new StartNewSimulationParameters() { decayGreen = 0.980f } });
            decayGreenCombo.Items.Add(new ComboBoxItem() { Content = "Medium plant scent radius (decay = 0.990)", Tag = new StartNewSimulationParameters() { decayGreen = 0.990f }, IsSelected = true });
            decayGreenCombo.Items.Add(new ComboBoxItem() { Content = "Far plant scent radius (decay = 0.994)", Tag = new StartNewSimulationParameters() { decayGreen = 0.994f } });
            decayGreenCombo.Items.Add(new ComboBoxItem() { Content = "Farther plant scent radius (decay = 0.997)", Tag = new StartNewSimulationParameters() { decayGreen = 0.997f } });
            decayGreenCombo.Items.Add(new ComboBoxItem() { Content = "Farthest plant scent radius (decay = 0.999)", Tag = new StartNewSimulationParameters() { decayGreen = 0.999f } });

            ComboBox decayBlueCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            decayBlueCombo.Items.Add(new ComboBoxItem() { Content = "Shortest prey scent radius (decay = 0.950)", Tag = new StartNewSimulationParameters() { decayBlue = 0.950f } });
            decayBlueCombo.Items.Add(new ComboBoxItem() { Content = "Shorter prey scent radius (decay = 0.970)", Tag = new StartNewSimulationParameters() { decayBlue = 0.970f } });
            decayBlueCombo.Items.Add(new ComboBoxItem() { Content = "Short prey scent radius (decay = 0.950)", Tag = new StartNewSimulationParameters() { decayBlue = 0.950f } });
            decayBlueCombo.Items.Add(new ComboBoxItem() { Content = "Medium prey scent radius (decay = 0.990)", Tag = new StartNewSimulationParameters() { decayBlue = 0.990f } });
            decayBlueCombo.Items.Add(new ComboBoxItem() { Content = "Far prey scent radius (decay = 0.994)", Tag = new StartNewSimulationParameters() { decayBlue = 0.994f }, IsSelected = true });
            decayBlueCombo.Items.Add(new ComboBoxItem() { Content = "Farther prey scent radius (decay = 0.997)", Tag = new StartNewSimulationParameters() { decayBlue = 0.997f } });
            decayBlueCombo.Items.Add(new ComboBoxItem() { Content = "Farthest prey scent radius (decay = 0.999)", Tag = new StartNewSimulationParameters() { decayBlue = 0.999f } });

            ComboBox decayRedCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            decayRedCombo.Items.Add(new ComboBoxItem() { Content = "Shortest predator scent radius (decay = 0.950)", Tag = new StartNewSimulationParameters() { decayRed = 0.950f } });
            decayRedCombo.Items.Add(new ComboBoxItem() { Content = "Shorter predator scent radius (decay = 0.970)", Tag = new StartNewSimulationParameters() { decayRed = 0.970f } });
            decayRedCombo.Items.Add(new ComboBoxItem() { Content = "Short predator scent radius (decay = 0.980)", Tag = new StartNewSimulationParameters() { decayRed = 0.980f } });
            decayRedCombo.Items.Add(new ComboBoxItem() { Content = "Medium predator scent radius (decay = 0.990)", Tag = new StartNewSimulationParameters() { decayRed = 0.990f }, IsSelected = true });
            decayRedCombo.Items.Add(new ComboBoxItem() { Content = "Far predator scent radius (decay = 0.994)", Tag = new StartNewSimulationParameters() { decayRed = 0.994f } });
            decayRedCombo.Items.Add(new ComboBoxItem() { Content = "Farther predator scent radius (decay = 0.997)", Tag = new StartNewSimulationParameters() { decayRed = 0.997f } });
            decayRedCombo.Items.Add(new ComboBoxItem() { Content = "Farthest predator scent radius (decay = 0.999)", Tag = new StartNewSimulationParameters() { decayRed = 0.999f } });

            ComboBox blueMaxVelocityCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            blueMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Prey very slow (0.1)", Tag = new StartNewSimulationParameters() { blueMaxVelocity = 0.1f } });
            blueMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Prey slow (0.2)", Tag = new StartNewSimulationParameters() { blueMaxVelocity = 0.2f } });
            blueMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Prey medium velocity (0.3)", Tag = new StartNewSimulationParameters() { blueMaxVelocity = 0.3f }, IsSelected = true });
            blueMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Prey fast (0.5)", Tag = new StartNewSimulationParameters() { blueMaxVelocity = 0.5f } });
            blueMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Prey very fast (0.7)", Tag = new StartNewSimulationParameters() { blueMaxVelocity = 0.7f } });
            blueMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Prey fastest (1.0)", Tag = new StartNewSimulationParameters() { blueMaxVelocity = 1.0f } });

            ComboBox redMaxVelocityCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            redMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Predator very slow (0.1)", Tag = new StartNewSimulationParameters() { redMaxVelocity = 0.1f } });
            redMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Predator slow (0.2)", Tag = new StartNewSimulationParameters() { redMaxVelocity = 0.2f } });
            redMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Predator medium velocity (0.3)", Tag = new StartNewSimulationParameters() { redMaxVelocity = 0.3f } });
            redMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Predator fast (0.5)", Tag = new StartNewSimulationParameters() { redMaxVelocity = 0.5f }, IsSelected = true });
            redMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Predator very fast (0.7)", Tag = new StartNewSimulationParameters() { redMaxVelocity = 0.7f } });
            redMaxVelocityCombo.Items.Add(new ComboBoxItem() { Content = "Predator fastest (1.0)", Tag = new StartNewSimulationParameters() { redMaxVelocity = 1.0f } });

            ComboBox seedCombo = new ComboBox() { Margin = new Thickness(0, 5, 0, 0) };
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Initialize agents randomly with fixed seed", Tag = new StartNewSimulationParameters() { fixedSeed = true }, IsSelected = true });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Initialize agents randomly with random seed", Tag = new StartNewSimulationParameters() { fixedSeed = false } });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Use agents from current simulation", Tag = new StartNewSimulationParameters() { useExistingAgents = true } });
            seedCombo.Items.Add(new ComboBoxItem() { Content = "Load agents from (multiple) files", Tag = new StartNewSimulationParameters() { useExistingAgents = true, loadAgentsFromFiles = true } });

            // Buttons
            StackPanel buttonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Button ok = new Button() { Content = "Start!", Width = 70, Margin = new Thickness(5, 0, 0, 0) };
            Button cancel = new Button() { Content = "Cancel", Width = 70 };
            Button addFiles = new Button() { Content = "Add...", Width = 70, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Right, IsEnabled = false };
            TextBlock txtFiles = new TextBlock() { Text = "", Margin = new Thickness(0, 0, 0, 10), FontSize = 10, TextWrapping = TextWrapping.Wrap, Height = 50 };
            List<string> fileNames = new List<string>();
            seedCombo.SelectionChanged += (s, e) =>
            {
                var selectedSeed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(seedCombo.SelectedItem);
                addFiles.IsEnabled = selectedSeed.loadAgentsFromFiles;
                ok.IsEnabled = !selectedSeed.loadAgentsFromFiles || fileNames.Count > 0;
            };

           
            addFiles.Click += (s, e) =>
            {
                var openDialog = new CommonOpenFileDialog { Title = "Open simulation gz or json file", Multiselect = true };
                openDialog.Filters.Add(new CommonFileDialogFilter("Simulation files", "*.gz;*.json"));
                if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    fileNames.AddRange(openDialog.FileNames);
                txtFiles.Text = "Load agents from files: " + string.Join(",  ", fileNames.Select(fn=>Path.GetFileName(fn)));
                var selectedSeed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(seedCombo.SelectedItem);
                ok.IsEnabled = !selectedSeed.loadAgentsFromFiles || fileNames.Count > 0;
            };

            ok.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
            cancel.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(cancel);
            buttonPanel.Children.Add(ok);

            // Compose UI
            panel.Children.Add(txt);
            panel.Children.Add(sizeCombo);
            panel.Children.Add(countCombo);
            panel.Children.Add(proportionCombo);
            panel.Children.Add(decayGreenCombo);
            panel.Children.Add(decayBlueCombo);
            panel.Children.Add(decayRedCombo);
            panel.Children.Add(blueMaxVelocityCombo);
            panel.Children.Add(redMaxVelocityCombo);
            panel.Children.Add(seedCombo);
            panel.Children.Add(addFiles);
            panel.Children.Add(txtFiles);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            // Show input dialog
            if (dialog.ShowDialog() == true)
            {
                var selectedSize = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(sizeCombo.SelectedItem);
                var selectedCount = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(countCombo.SelectedItem);
                var selectedPoportion = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(proportionCombo.SelectedItem);
                var selectedSeed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(seedCombo.SelectedItem);
                var selectedDecayGreen = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(decayGreenCombo.SelectedItem);
                var selectedDecayBlue = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(decayBlueCombo.SelectedItem);
                var selectedDecayRed = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(decayRedCombo.SelectedItem);
                var selectedBlueMaxVelocity = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(blueMaxVelocityCombo.SelectedItem);
                var selectedRedMaxVelocity = WpfUtil.GetTagAsObject<StartNewSimulationParameters>(redMaxVelocityCombo.SelectedItem);
                return new StartNewSimulationParameters()
                {
                    agentsCount = selectedCount.agentsCount,
                    width = selectedSize.width,
                    height = selectedSize.height,
                    plantsRatio = selectedPoportion.plantsRatio,
                    predatorsRatio = selectedPoportion.predatorsRatio,
                    fixedSeed = selectedSeed.fixedSeed,
                    useExistingAgents = selectedSeed.useExistingAgents,
                    loadAgentsFromFiles = selectedSeed.loadAgentsFromFiles,
                    decayGreen = selectedDecayGreen.decayGreen,
                    decayBlue = selectedDecayBlue.decayBlue,
                    decayRed = selectedDecayRed.decayRed,
                    blueMaxVelocity = selectedBlueMaxVelocity.blueMaxVelocity,
                    redMaxVelocity = selectedRedMaxVelocity.redMaxVelocity,
                    fileNames = fileNames
                };
            }
            else
            {
                return null;
            }
        }
    }
}
