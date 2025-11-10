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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StartupWorkspace codeworkWorkspace = new StartupWorkspace() { WorkspaceName = "Code Work" };
        StartupWorkspace tradingWorkspace = new StartupWorkspace() { WorkspaceName = "Trading" };

        WorkspaceFile workspaceFile = null;

        public MainWindow()
        {
            App app = (App)Application.Current;
            if(app == null)
            {
                MessageBox.Show("Application is null");
                Application.Current.Shutdown(1);
                return;
            }
            workspaceFile = app.WorkspaceFile;
            if (workspaceFile == null)
            {
                MessageBox.Show("Workspace file is null");
                Application.Current.Shutdown(2);
                return;
            }

            InitializeComponent();

            List<StartupWorkspace> validWorkspaces = workspaceFile.DetectWorkspaces();
            string currentWorkspace = workspaceFile.LookupCurrentWorkspace();


            //////////////////
            /// replace this with dynamic radio button creation based on the detected workspaces
            //////////////////
            CodeWorkButton.Tag = codeworkWorkspace;
            TradingRadioButton.Tag = tradingWorkspace;
                        
            if (currentWorkspace == codeworkWorkspace.WorkspaceName)
            {
                CodeWorkButton.IsChecked = true;
            }
            else if (currentWorkspace == tradingWorkspace.WorkspaceName)
            {
                TradingRadioButton.IsChecked = true;
            }
            //////////////////
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (workspaceFile.LaunchNinjaTrader())
            {
                Application.Current.Shutdown();
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            StartupWorkspace workspace = radioButton.Tag as StartupWorkspace;
            if (workspace == null)
            {
                MessageBox.Show("Error: Workspace radio button has no StartupWorkspace tag associated.");
                return;
            }

            string result = workspaceFile.SetStartupWorkspace(workspace);
            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show($"Error: Setting startup workspace to \"{workspace.WorkspaceName}\" failed: {result}");
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}