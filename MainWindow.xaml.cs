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

namespace NinjaTraderLauncher
{
    public partial class MainWindow : Window
    {
        WorkspaceFile? _workspaceFile = null;
        NinjaTraderCleaner _cleaner;
        App? _application;

        public MainWindow()
        {
            _cleaner = new NinjaTraderCleaner(new LauncherOptions().NinjaTraderDocumentsDirectory);

            _application = Application.Current as App;
            if (_application == null)
            {
                MessageBox.Show("Application is null");
                Application.Current.Shutdown(1);
                return;
            }

            _workspaceFile = _application.WorkspaceFile;
            if (_workspaceFile == null)
            {
                MessageBox.Show("Workspace file is null");
                Application.Current.Shutdown(2);
                return;
            }

            InitializeComponent();
            SafeModeCheckBox.IsChecked = _application.SafeMode;
            SafeModeCheckBox.Checked += SafeModeCheckBox_Checked;
            SafeModeCheckBox.Unchecked += SafeModeCheckBox_Unchecked;

            List<StartupWorkspace> validWorkspaces = _workspaceFile.DetectWorkspaces();
            string currentWorkspace = _workspaceFile.LookupCurrentWorkspace();
            int n = 0;
            foreach (StartupWorkspace workspace in validWorkspaces)
            {
                RadioButton rb = new RadioButton();
                rb.Name = workspace.WorkspaceName.Replace(' ', '_') + "_RadioButton";
                rb.Content = workspace.WorkspaceName;
                rb.Tag = workspace;
                if(currentWorkspace ==  workspace.WorkspaceName) rb.IsChecked = true;
                rb.Checked += RadioButton_Checked;
                Thickness t = new Thickness();
                t.Left = 27;
                rb.Margin = t;
                RowDefinition rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                LauncherControlGrid.RowDefinitions.Add(rd);

                LauncherControlGrid.Children.Add(rb);
                Grid.SetRow(rb, n++);
            }
        }

        private void SafeModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _application!.SafeMode = false;
        }

        private void SafeModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _application!.SafeMode = true;
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if(_workspaceFile == null) return;

            if (_workspaceFile.LaunchNinjaTrader(_application!.SafeMode))
            {
                Application.Current.Shutdown();
            }
        }

        private void LaunchAndCleanButton_Click(object sender, RoutedEventArgs e)
        {
            if (_workspaceFile == null) return;

            performCleanup();

            if (_workspaceFile.LaunchNinjaTrader(_application!.SafeMode))
            {
                Application.Current.Shutdown();
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton? radioButton = sender as RadioButton;
            StartupWorkspace? workspace = radioButton?.Tag as StartupWorkspace;
            if (workspace == null)
            {
                MessageBox.Show("Error: Workspace radio button has no StartupWorkspace tag associated.");
                return;
            }

            string? result = _workspaceFile?.SetStartupWorkspace(workspace);
            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show($"Error: Setting startup workspace to \"{workspace.WorkspaceName}\" failed: {result}");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void cleanAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool isAllChecked = cleanAllCheckBox.IsChecked == null ? false : ((bool)cleanAllCheckBox.IsChecked);
            if (isAllChecked)
            {
                cleanLogCheckBox.IsEnabled = false;
                cleanTraceCheckBox.IsEnabled = false;
                cleanCacheCheckBox.IsEnabled = false;
                cleanDBCheckBox.IsEnabled = false;
            }
            else
            {
                cleanLogCheckBox.IsEnabled = true;
                cleanTraceCheckBox.IsEnabled = true;
                cleanCacheCheckBox.IsEnabled = true;
                cleanDBCheckBox.IsEnabled = true;
            }
        }

        private void cleanButton_Click(object sender, RoutedEventArgs e)
        {
            performCleanup();
        }

        private void performCleanup()
        { 
            bool isAllChecked = cleanAllCheckBox.IsChecked == null ? false : ((bool)cleanAllCheckBox.IsChecked);
            

            if (isAllChecked)
            {
                if (!_cleaner.CleanAll())
                {
                    MessageBox.Show(_cleaner.Error, "Clean Error");
                    return;
                }
            }
            else
            {
                bool isCacheChecked = cleanCacheCheckBox.IsChecked == null ? false : ((bool)cleanCacheCheckBox.IsChecked);
                bool isLogChecked = cleanLogCheckBox.IsChecked == null ? false : ((bool)cleanLogCheckBox.IsChecked);
                bool isTraceChecked = cleanTraceCheckBox.IsChecked == null ? false : ((bool)cleanTraceCheckBox.IsChecked);
                bool isDBChecked = cleanDBCheckBox.IsChecked == null ? false : ((bool)cleanDBCheckBox.IsChecked);

                if (isCacheChecked && !_cleaner.CleanupCache())
                {
                    MessageBox.Show(_cleaner.Error, "Cache Clean Error");
                    return;
                }
                if (isLogChecked && !_cleaner.CleanupLogs())
                {
                    MessageBox.Show(_cleaner.Error, "Log Clean Error");
                    return;
                }
                if (isTraceChecked && !_cleaner.CleanupTraces())
                {
                    MessageBox.Show(_cleaner.Error, "Trace Clean Error");
                    return;
                }
                if (isDBChecked && !_cleaner.CleanupDB())
                {
                    MessageBox.Show(_cleaner.Error, "DB Clean Error");
                    return;
                }

            }

        }
    }
}