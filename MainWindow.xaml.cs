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
        WorkspaceFile workspaceFile = new WorkspaceFile() { FilePath= "C:\\Users\\Mark\\Documents\\NinjaTrader 8\\workspaces\\_Workspaces.xml" };
        StartupWorkspace codeworkWorkspace = new StartupWorkspace() { WorkspaceName = "Code Work" };
        StartupWorkspace tradingWorkspace = new StartupWorkspace() { WorkspaceName = "Trading" };
        string NinjaTraderExecutable = "C:\\Program Files\\NinjaTrader 8\\bin\\NinjaTrader.exe";

        public MainWindow()
        {
            InitializeComponent();

            string currentWorkspace = workspaceFile.LookupCurrentWorkspace();
            if (currentWorkspace == codeworkWorkspace.WorkspaceName)
            {
                CodeWorkButton.IsChecked = true;
            }
            else if (currentWorkspace == tradingWorkspace.WorkspaceName)
            {
                TradingRadioButton.IsChecked = true;
            }
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            bool started = false;
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = NinjaTraderExecutable;
                pProcess.StartInfo.UseShellExecute = false;
                started = pProcess.Start();
            }
            if (started)
            {
                Application.Current.Shutdown();
            }
        }

        private void CodeWorkButton_Checked(object sender, RoutedEventArgs e)
        {
            string result = workspaceFile.SetStartupWorkspace(codeworkWorkspace);
            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show($"Error setting startup workspace to Code Work: {result}");
            }
        }

        private void TradingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string result = workspaceFile.SetStartupWorkspace(tradingWorkspace);
            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show($"Error setting startup workspace to Trading: {result}");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}