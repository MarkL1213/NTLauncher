using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;

namespace NinjaTraderLauncher
{
    public class LauncherOptions
    {
        public const string NinjaTraderExecutable = "C:\\Program Files\\NinjaTrader 8\\bin\\NinjaTrader.exe";
        public const string NinjaTraderDocumentsDirectory = "C:\\Users\\Mark\\Documents\\NinjaTrader 8";
    }

    public class StartupWorkspace
    {
        public string WorkspaceName { get; set; }
    }

    public class WorkspaceFile
    {
        public string ConfigFileName { get; set; }
        public string FilePath { get; set; }

        public bool LaunchNinjaTrader()
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = LauncherOptions.NinjaTraderExecutable;
                pProcess.StartInfo.UseShellExecute = false;
                return pProcess.Start();
            }
        }

        public string SetStartupWorkspace(StartupWorkspace workspace)
        {
            string fullPath = Path.Combine(FilePath, ConfigFileName);

            if (!File.Exists(fullPath))
            {
                return "File does not exist";
            }

            try
            {
                string[] lines = File.ReadAllLines(fullPath);
                int lineIndex = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("  <ActiveWorkspace>", StringComparison.CurrentCulture))
                    {
                        lineIndex = i;
                        break;
                    }
                }
                if (lineIndex == -1)
                {
                    return "ActiveWorkspace tag not found in file";
                }
                lines[lineIndex] = $"  <ActiveWorkspace>{workspace.WorkspaceName}</ActiveWorkspace>";

                File.WriteAllLines(fullPath, lines);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        public List<StartupWorkspace> DetectWorkspaces()
        {
            List<StartupWorkspace> workspaces = new List<StartupWorkspace>();
            if (!Directory.Exists(FilePath))
            {
                return workspaces;
            }
            try
            {
                Directory.EnumerateFiles(FilePath, "*.xml").ToList().ForEach(file =>
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName != ConfigFileName)
                    {
                        string fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
                        workspaces.Add(new StartupWorkspace() { WorkspaceName = fileNameNoExtension });
                    }
                });
            }
            catch
            {
                return workspaces;
            }
            return workspaces;
        }

        public string LookupCurrentWorkspace()
        {
            string fullPath = Path.Combine(FilePath, ConfigFileName);

            if (!File.Exists(fullPath))
            {
                return string.Empty;
            }
            try
            {
                string[] lines = File.ReadAllLines(fullPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("  <ActiveWorkspace>", StringComparison.CurrentCulture))
                    {
                        int startIndex = line.IndexOf('>') + 1;
                        int endIndex = line.IndexOf("</ActiveWorkspace>");
                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            return line.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }

    public class NinjaTraderCleaner
    {
        public NinjaTraderCleaner() { }
        public NinjaTraderCleaner(string installDirectory) { InstallDirectory = installDirectory; }

        public enum DataInterval { Tick, Minute, Day, Replay, Cache, All };

        public string InstallDirectory { set; get; }

        public string Error { get; private set; }

        public bool VerifyInstallDirectory()
        {
            Error = string.Empty;
            if (string.IsNullOrEmpty(InstallDirectory))
            {
                Error = "Install directory is not set.";
                return false;
            }
            if (!Directory.Exists(InstallDirectory))
            {
                Error = $"Install directory '{InstallDirectory}' does not exist.";
                return false;
            }
            return true;
        }

        private bool DeleteAllRecursive(string directoryToClean)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(directoryToClean);

                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch(Exception e)
            {
                Error = e.Message;
                return false;
            }
            return true;
        }

        public bool CleanAll()
        {
            if (!CleanupCache()) return false;
            if (!CleanupDB()) return false;
            if (!CleanupLogs()) return false;
            if (!CleanupTraces()) return false;
            if (!CleanupStrategyAnalyzerLogs()) return false;
            return true;
        }

        public bool CleanupStrategyAnalyzerLogs()
        {
            Error = string.Empty;
            if (!VerifyInstallDirectory()) return false;

            string logDirectory = Path.Combine(InstallDirectory, "strategyanalyzerlogs\\");
            if (!Directory.Exists(logDirectory))
            {
                Error = $"Strategy Analyzer log directory '{logDirectory}' does not exist.";
                return false;
            }
            return DeleteAllRecursive(logDirectory);
        }

        public bool CleanupLogs()
        {
            Error = string.Empty;
            if (!VerifyInstallDirectory()) return false;

            string logDirectory = Path.Combine(InstallDirectory,"log\\");
            if (!Directory.Exists(logDirectory))
            {
                Error = $"Log directory '{logDirectory}' does not exist.";
                return false;
            }
            return DeleteAllRecursive(logDirectory);
        }

        public bool CleanupTraces()
        {
            Error = string.Empty;
            if (!VerifyInstallDirectory()) return false;

            string traceDirectory = Path.Combine(InstallDirectory, "trace\\");
            if (!Directory.Exists(traceDirectory))
            {
                Error = $"Trace directory '{traceDirectory}' does not exist.";
                return false;
            }
            return DeleteAllRecursive(traceDirectory);
        }

        public bool CleanupCache()
        {
            Error = string.Empty;
            if (!VerifyInstallDirectory()) return false;

            string cacheDirectory = Path.Combine(InstallDirectory, "cache\\");
            if (!Directory.Exists(cacheDirectory))
            {
                Error = $"Cache directory '{cacheDirectory}' does not exist.";
                return false;
            }
            return DeleteAllRecursive(cacheDirectory);
        }

        public bool CleanupDB(DataInterval interval = DataInterval.All)
        {
            Error = string.Empty;
            if (!VerifyInstallDirectory()) return false;

            string dbDirectory = string.Empty;
            switch (interval)
            {
                case DataInterval.All:
                    if (!CleanupDB(DataInterval.Tick)) return false;
                    if (!CleanupDB(DataInterval.Minute)) return false;
                    if (!CleanupDB(DataInterval.Day)) return false;
                    if (!CleanupDB(DataInterval.Replay)) return false;
                    if (!CleanupDB(DataInterval.Cache)) return false;
                    return true;
                case DataInterval.Tick:
                    dbDirectory = Path.Combine(InstallDirectory, "db\\tick\\");
                    break;
                case DataInterval.Minute:
                    dbDirectory = Path.Combine(InstallDirectory, "db\\minute\\");
                    break;
                case DataInterval.Day:
                    dbDirectory = Path.Combine(InstallDirectory, "db\\day\\");
                    break;
                case DataInterval.Replay:
                    dbDirectory = Path.Combine(InstallDirectory, "db\\replay\\");
                    break;
                case DataInterval.Cache:
                    dbDirectory = Path.Combine(InstallDirectory, "db\\cache\\");
                    break;
            }

            if (string.IsNullOrEmpty(dbDirectory))
            {
                Error = $"Database interval unknown.";
                return false;
            }

            if (!Directory.Exists(dbDirectory))
            {
                Error = $"Database directory '{dbDirectory}' does not exist.";
                return false;
            }

            return DeleteAllRecursive(dbDirectory);
        }

    }
}
