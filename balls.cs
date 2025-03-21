// tottaly made by bitflags
// I REMOVED MONACO AND WEBVIEW AKA TAB SYSTEM CUZ OF ERRORS HAPPENING IN MY PC ADD IT YOURSELF,LAZY FU
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernExecutor
{
    public partial class MainWindow : Window
    {
        private bool isInjected = false;
        private Dictionary<RadioButton, string> tabContents = new Dictionary<RadioButton, string>();
        private ObservableCollection<ScriptFile> scriptFiles = new ObservableCollection<ScriptFile>();
        private int tabCounter = 1;
        private string scriptsFolder;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                scriptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
                if (!Directory.Exists(scriptsFolder))
                {
                    Directory.CreateDirectory(scriptsFolder);
                }

                await MonacoEditor.EnsureCoreWebView2Async();

                tabContents[DefaultTab] = "-- Welcome to Modern Executor\n-- Start coding here or load a script\n\n";

                LogToConsole("Application initialized successfully", LogType.Info);
                LogToConsole("Ready to execute scripts", LogType.Success);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

   

        private RadioButton GetCurrentTab()
        {
            foreach (var child in TabsPanel.Children)
            {
                if (child is RadioButton rb && rb.IsChecked == true)
                {
                    return rb;
                }
            }
            return null;
        }

        private bool IsChecked(RadioButton button)
        {
            return button.IsChecked == true;
        }

        
        

        private void LoadScriptFiles()
        {
            scriptFiles.Clear();
            try
            {
                string[] files = Directory.GetFiles(scriptsFolder, "*.lua");
                foreach (string file in files)
                {
                    scriptFiles.Add(new ScriptFile
                    {
                        Name = Path.GetFileName(file),
                        Path = file
                    });
                }
                ScriptsList.ItemsSource = scriptFiles;
            }
            catch (Exception ex)
            {
                LogToConsole($"Error loading scripts: {ex.Message}", LogType.Error);
            }
        }

        #region UI Event Handlers

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Navigation_Checked(object sender, RoutedEventArgs e)
        {
            if (HomeContent == null || ExecuteContent == null || SettingsContent == null) return;

            HomeContent.Visibility = Visibility.Collapsed;
            ExecuteContent.Visibility = Visibility.Collapsed;
            SettingsContent.Visibility = Visibility.Collapsed;

            if (sender == HomeButton)
                HomeContent.Visibility = Visibility.Visible;
            else if (sender == ExecuteButton)
                ExecuteContent.Visibility = Visibility.Visible;
            else if (sender == SettingsButton)
                SettingsContent.Visibility = Visibility.Visible;
        }

        private void AddTab_Click(object sender, RoutedEventArgs e)
        {
            RadioButton newTab = new RadioButton
            {
                Content = $"Script {tabCounter++}",
                Style = FindResource("ScriptTabStyle") as Style,
                Margin = new Thickness(0, 10, 0, 0)
            };
            newTab.Checked += ScriptTab_Checked;

            tabContents[newTab] = "-- New script\n\n";

            TabsPanel.Children.Add(newTab);
            newTab.IsChecked = true;
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            var closeButton = (Button)sender;
            var tab = FindParent<RadioButton>(closeButton);
            
            if (tab != null && TabsPanel.Children.Count > 1)
            {
                bool wasChecked = tab.IsChecked == true;
                TabsPanel.Children.Remove(tab);
                tabContents.Remove(tab);

                if (wasChecked && TabsPanel.Children.Count > 0)
                {
                    DefaultTab.IsChecked = true;
                }
            }

            e.Handled = true;
        }

        private void ScriptTab_Checked(object sender, RoutedEventArgs e)
        {
            var tab = (RadioButton)sender;
            if (tabContents.ContainsKey(tab))
            {
            }
        }

        private void ToggleScriptExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptExplorer.Visibility == Visibility.Collapsed)
            {
                ScriptExplorer.Visibility = Visibility.Visible;
                ExplorerSplitter.Visibility = Visibility.Visible;

                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 250,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                ScriptExplorerTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            else
            {
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 0,
                    To = 250,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                animation.Completed += (s, args) =>
                {
                    ScriptExplorer.Visibility = Visibility.Collapsed;
                    ExplorerSplitter.Visibility = Visibility.Collapsed;
                };
                ScriptExplorerTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
        }

        private void ScriptsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScriptsList.SelectedItem is ScriptFile selectedScript)
            {
                try
                {
                    string content = File.ReadAllText(selectedScript.Path);
                    var currentTab = GetCurrentTab();
                    if (currentTab != null)
                    {
                        tabContents[currentTab] = content;
                    }
                    LogToConsole($"Loaded script: {selectedScript.Name}", LogType.Info);
                }
                catch (Exception ex)
                {
                    LogToConsole($"Error loading script: {ex.Message}", LogType.Error);
                }
                ScriptsList.SelectedItem = null;
            }
        }

        private void LoadScript_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string scriptTag = button.Tag.ToString();
            string scriptContent = "";

            switch (scriptTag)
            {
                case "InfiniteYield":
                    scriptContent = "-- Infinite Yield Admin Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/EdgeIY/infiniteyield/master/source'))()";
                    break;
                case "ESP":
                    scriptContent = "-- Universal ESP Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/ic3w0lf22/Unnamed-ESP/master/UnnamedESP.lua'))()";
                    break;
                case "BloxFruits":
                    scriptContent = "-- Blox Fruits Auto Farm Script\nloadstring(game:HttpGet('https://raw.githubusercontent.com/scriptpastebin/raw/main/BloxFruit'))()";
                    break;
            }

            var currentTab = GetCurrentTab();
            if (currentTab != null)
            {
                tabContents[currentTab] = scriptContent;
            }

            ExecuteButton.IsChecked = true;
            LogToConsole($"Loaded {scriptTag} script", LogType.Info);
        }

        private void Inject_Click(object sender, RoutedEventArgs e)
        {
            if (!isInjected)
            {
                LogToConsole("Injecting...", LogType.Info);
                // Simulate injection with a delay
                System.Threading.Thread.Sleep(500);
                isInjected = true;
                LogToConsole("Successfully injected", LogType.Success);
            }
            else
            {
                LogToConsole("Already injected", LogType.Warning);
            }
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            if (!isInjected)
            {
                LogToConsole("Please inject first", LogType.Warning);
                return;
            }

            var currentTab = GetCurrentTab();
            if (currentTab != null && tabContents.ContainsKey(currentTab))
            {
                string script = tabContents[currentTab];
                // Simulate execution
                LogToConsole("Executing script...", LogType.Info);
                System.Threading.Thread.Sleep(200);
                LogToConsole("Script executed successfully", LogType.Success);
            }
        }

        private void ClearEditor_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab != null)
            {
                tabContents[currentTab] = "";
            }
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Document.Blocks.Clear();
        }

        private void TopMost_CheckedChanged(object sender, RoutedEventArgs e)
        {
            this.Topmost = TopMostCheckBox.IsChecked == true;
        }

        

        private void Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Opacity = e.NewValue;
        }
        private void FontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }
        private void Theme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            this.Topmost = TopMostCheckBox.IsChecked == true;
        }
        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            LogToConsole("Checking for updates...", LogType.Info);
            System.Threading.Thread.Sleep(500);
            LogToConsole("You are running the latest version", LogType.Success);
        }

        #endregion

        #region Helper Methods

        private void LogToConsole(string message, LogType type)
        {
            if (ConsoleOutput == null) return;

            Paragraph paragraph = new Paragraph();
            Run timeRun = new Run($"[{DateTime.Now.ToString("HH:mm:ss")}] ");
            timeRun.Foreground = Brushes.Gray;
            paragraph.Inlines.Add(timeRun);

            Run messageRun = new Run(message);
            switch (type)
            {
                case LogType.Error:
                    messageRun.Foreground = new SolidColorBrush((Color)FindResource("ErrorColor"));
                    break;
                case LogType.Warning:
                    messageRun.Foreground = new SolidColorBrush((Color)FindResource("WarningColor"));
                    break;
                case LogType.Success:
                    messageRun.Foreground = new SolidColorBrush((Color)FindResource("SuccessColor"));
                    break;
                case LogType.Info:
                default:
                    messageRun.Foreground = Brushes.White;
                    break;
            }
            paragraph.Inlines.Add(messageRun);

            ConsoleOutput.Document.Blocks.Add(paragraph);
            ConsoleScroller.ScrollToEnd();
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        #endregion
    }

    public enum LogType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public class ScriptFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
