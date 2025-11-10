using System.Configuration;
using System.Data;
using System.Security.Cryptography.X509Certificates;
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
        
        void AppStartup (object sender, StartupEventArgs e)
        {
            for (int i = 0; i != e.Args.Length; ++i)
            {
                string[] argValues = e.Args[i].Split('=');
                string argName = argValues[0];
                string argValue = string.Empty;

                if (argName == "/Launch" || argName == "-Launch" || argName == "--Launch")
                {
                    if (argValues.Length == 2) { argValue = argValues[1]; }
                    else
                    {
                        int n = 1;
                        if (i + n >= e.Args.Length)
                        {
                            MessageBox.Show("CommandLine argument \"Launch\" requires a values.");
                            Shutdown(3);
                            return;
                        }

                        if (e.Args[i + n] == "=") { n++; }

                        if (i + n >= e.Args.Length)
                        {
                            MessageBox.Show("CommandLine argument \"Launch\" requires a values.");
                            Shutdown(3);
                            return;

                        }

                        if (e.Args[i + n].StartsWith('/') || e.Args[i + n].StartsWith('-') || e.Args[i + n].StartsWith("--"))
                        {
                            MessageBox.Show("CommandLine argument \"Launch\" requires a values.");
                            Shutdown(3);
                            return;
                        }

                        argValue = e.Args[i + n];
                        i += n;
                    }

                    if (string.IsNullOrEmpty(argValue))
                    {
                        MessageBox.Show("CommandLine argument \"Launch\" requires a values.");
                        Shutdown(3);
                        return;
                    }

                    argValue = argValue.TrimStart();
                    argValue = argValue.TrimEnd();

                    if (string.IsNullOrEmpty(argValue))
                    {
                        MessageBox.Show("CommandLine argument \"Launch\" requires a values.");
                        Shutdown(3);
                        return;
                    }

                    if (argValue.StartsWith('"') && argValue.EndsWith('"'))
                    {
                        argValue = argValue.TrimStart('"');
                        argValue = argValue.TrimEnd('"');
                    }

                    if (string.IsNullOrEmpty(argValue))
                    {
                        MessageBox.Show("CommandLine argument \"Launch\" requires a values.");
                        Shutdown(3);
                        return;
                    }

                    List<StartupWorkspace> validWorkspaces = _workspaceFile.DetectWorkspaces();
                    if (validWorkspaces.Find(x => x.WorkspaceName == argValue) == null)
                    {
                        MessageBox.Show($"Unable to launch unknown workspace \"{argValue}\"");
                        Shutdown(5);
                    }

                    string result = _workspaceFile.SetStartupWorkspace(new StartupWorkspace() { WorkspaceName = argValue });
                    if (!string.IsNullOrEmpty(result))
                    {
                        MessageBox.Show(result);
                        Shutdown(6);
                        return;
                    }

                    if (!_workspaceFile.LaunchNinjaTrader())
                    {
                        MessageBox.Show("Failed to launch NinjaTrader application.");
                        Shutdown(7);
                        return;
                    }

                    Shutdown(0);
                    return;
                }
                else
                {
                    MessageBox.Show($"CommandLine argument \"{argName}\" unknown.");
                    Shutdown(4);
                    return;
                }

            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
