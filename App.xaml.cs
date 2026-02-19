using System.Configuration;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;


namespace NinjaTraderLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        WorkspaceFile _workspaceFile = new WorkspaceFile() { ConfigFileName = "_Workspaces.xml", FilePath = "C:\\Users\\Mark\\Documents\\NinjaTrader 8\\workspaces\\" };
        public WorkspaceFile WorkspaceFile { get { return _workspaceFile; } }

        void AppStartup(object sender, StartupEventArgs e)
        {
            CommandLine commandLine = new CommandLine();
            commandLine.AddPrefixes(new string[] { "/", "-", "--" });

            CommandLineArgument newArgument;
            newArgument = new CommandLineArgument("Launch", "Workspace name to launch", true);
            newArgument.UsesCharacter = true;
            newArgument.Character = 'l';
            commandLine.AddArgument(newArgument);
            newArgument = new CommandLineArgument("Help", "Display command line help.");
            newArgument.UsesCharacter = true;
            newArgument.Character = 'h';
            commandLine.AddArgument(newArgument);
            newArgument = new CommandLineArgument("Safe", "Set safe mode for launch.");
            newArgument.UsesCharacter = true;
            newArgument.Character = 's';
            commandLine.AddArgument(newArgument);

            if (!commandLine.Parse(e.Args))
            {
                MessageBox.Show(commandLine.AllErrors());
                Shutdown(10);
                return;
            }

            if (commandLine.Arguments["Help"].ArgumentFound)
            {
                bool isFirstLine = true;
                StringBuilder sb = new StringBuilder();
                

                foreach (string argName in commandLine.Arguments.Keys)
                {
                    CommandLineArgument arg = commandLine.Arguments[argName];
                    if (isFirstLine)
                    { sb.Append(arg.ToString()); isFirstLine = false; }
                    else
                        sb.Append("\r\n" + arg.ToString());
                }

                MessageBox.Show(sb.ToString());

                Shutdown(0);
                return;
            }

            if (commandLine.Arguments["Safe"].ArgumentFound)
            {
                SafeMode = true;
            }

            if (commandLine.Arguments["Launch"].ArgumentFound)
            {
                HandleLaunchArgument(commandLine.Arguments["Launch"]);
                return;
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        public bool SafeMode { get; set; } = false;


        private bool HandleLaunchArgument(CommandLineArgument launch)
        {
            List<StartupWorkspace> validWorkspaces = _workspaceFile.DetectWorkspaces();
            if (validWorkspaces.Find(x => x.WorkspaceName == launch.Value) == null)
            {
                MessageBox.Show($"Unable to launch unknown workspace \"{launch.Value}\"");
                Shutdown(5);
                return false;
            }

            string result = _workspaceFile.SetStartupWorkspace(new StartupWorkspace() { WorkspaceName = launch.Value });
            if (!string.IsNullOrEmpty(result))
            {
                MessageBox.Show(result);
                Shutdown(6);
                return false;
            }

            if (!_workspaceFile.LaunchNinjaTrader(SafeMode))
            {
                MessageBox.Show("Failed to launch NinjaTrader application.");
                Shutdown(7);
                return false;
            }

            Shutdown(0);
            return true;
        }
    }

}
