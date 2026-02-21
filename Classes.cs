using Microsoft.Win32;
using System.IO;

namespace NinjaTraderLauncher
{
    public class LauncherOptions
    {
        public LauncherOptions()
        {

        }

        private string GetInstallDirectory()
        {
            string regKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\NinjaTrader, LLC\\NinjaTrader";
            string? regValue = Registry.GetValue(regKey,"InstallDir",string.Empty) as string;
            if (string.IsNullOrEmpty(regValue)) { return string.Empty; }
            return regValue!;
        }

        public string NinjaTreaderDBDirectory { get { return Path.Combine(NinjaTraderDocumentsDirectory, "db"); } }
        public string NinjaTraderExecutable { get { return Path.Combine(GetInstallDirectory(),"bin","NinjaTrader.exe"); } }
        public string NinjaTraderDocumentsDirectory { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"NinjaTrader 8"); }
}
    }

    public class StartupWorkspace
    {
        public string WorkspaceName { get; set; } = string.Empty;
    }

    public class WorkspaceFile
    {
        public string ConfigFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public bool LaunchNinjaTrader(bool safeMode)
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                LauncherOptions lo = new LauncherOptions();
                pProcess.StartInfo.FileName = lo.NinjaTraderExecutable;
                pProcess.StartInfo.UseShellExecute = false;
                if(safeMode) pProcess.StartInfo.Arguments = "-safe";
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
        public NinjaTraderCleaner(string installDirectory) { InstallDirectory = installDirectory; }

        public enum DataInterval { Tick, Minute, Day, Replay, Cache, All };

        public string InstallDirectory { set; get; }

        public string Error { get; private set; } = string.Empty;

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
            catch (Exception e)
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

            string logDirectory = Path.Combine(InstallDirectory, "log\\");
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

    public class NinjaTraderDatabaseManager
    {
        public const string NTDBDirectory = "C:\\Users\\Mark\\Documents\\NinjaTrader 8\\db\\";
        public const string ManagerDBDirectory = "C:\\Users\\Mark\\Documents\\NinjaTrader 8 Backup\\db\\";

        private string _ntdbDirectory;
        private NinjaTraderDatabaseManager(string ntdbDirectory = NTDBDirectory)
        {
            _ntdbDirectory = ntdbDirectory;
        }
        public enum DataInterval { Tick, Minute, Day, Replay }

        public string Error { get; private set; } = string.Empty;

        public string GetIntervalFolder(string DBRoot,DataInterval interval)
        {
            string dir = string.Empty;
            switch (interval)
            {
                case DataInterval.Tick: dir = Path.Combine(_ntdbDirectory, "tick\\"); break;
                case DataInterval.Day: dir = Path.Combine(_ntdbDirectory, "day\\"); break;
                case DataInterval.Minute: dir = Path.Combine(_ntdbDirectory, "minute\\"); break;
                case DataInterval.Replay: dir = Path.Combine(_ntdbDirectory, "replay\\"); break;
            }
            return dir;
        }

        List<string> FindAllSymbols(DataInterval interval)
        {
            Error = string.Empty;
            List<string> symbols = new List<string>();
            string dir = GetIntervalFolder(_ntdbDirectory, interval);
            if (!Directory.Exists(dir))
            {
                Error = $"Database directory '{dir}' does not exist.";
                return symbols;
            }

            DirectoryInfo dbInfo = new DirectoryInfo(dir);
            foreach (DirectoryInfo di in dbInfo.EnumerateDirectories(dir)) {
                symbols.Add(di.Name);
            }

            return symbols;
        }

        //////////
        //need utility functions to move specific data files into and out of the NT DB
        //////////
        //BackupAllDataFiles(symbol="")
        public bool BackupAllDataFiles(string symbolToBackup = "")
        {
            if (!BackupAllDataFiles(DataInterval.Tick, symbolToBackup)) return false;
            if (!BackupAllDataFiles(DataInterval.Minute, symbolToBackup)) return false;
            if (!BackupAllDataFiles(DataInterval.Day, symbolToBackup)) return false;
            return BackupAllDataFiles(DataInterval.Replay, symbolToBackup);
        }

        public bool BackupAllDataFiles(DataInterval interval, string symbolToBackup = "")
        {
            Error = string.Empty;
            List<string> symbols;
            if (string.IsNullOrEmpty(symbolToBackup))
            {
                symbols = FindAllSymbols(interval);
                if (!string.IsNullOrEmpty(Error)) return false;
            }
            else
            {
                symbols = new List<string>();
                symbols.Add(symbolToBackup);
            }

            string intervalFolder = GetIntervalFolder(_ntdbDirectory, interval);
            string backupIntervalFolder = GetIntervalFolder(ManagerDBDirectory, interval);

            foreach (string symbol in symbols)
            {
                string symbolFolderName = Path.Combine(intervalFolder, symbol);
                DirectoryInfo si = new DirectoryInfo(symbolFolderName);
                if (!si.Exists)
                {
                    Error = "";
                    return false;
                }

                string backupSymbolFolderName = Path.Combine(backupIntervalFolder, symbol);
                DirectoryInfo bsi = new DirectoryInfo(backupSymbolFolderName);
                if (!bsi.Exists)
                {
                    try
                    {
                        bsi.Create();
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;
                        return false;
                    }
                }

                foreach (FileInfo fi in si.EnumerateFiles())
                {
                    string backupFileName = Path.Combine(backupSymbolFolderName, fi.Name);
                    FileInfo bfi = new FileInfo(backupFileName);
                    bool backupCreated = false;
                    string tmpBackupFile = bfi.FullName + ".tmp";
                    if (bfi.Exists)
                    {
                        try
                        {
                            if (File.Exists(tmpBackupFile))
                            {
                                File.Delete(tmpBackupFile);
                            }
                            File.Move(bfi.FullName, tmpBackupFile);
                        }
                        catch (Exception ex)
                        {
                            Error = ex.Message;
                            return false;
                        }
                        backupCreated = true;
                    }

                    try
                    {
                        bfi = fi.CopyTo(backupFileName);
                    }
                    catch (Exception ex)
                    {
                        Error = ex.Message;
                        if (backupCreated)
                        {
                            File.Move(tmpBackupFile, backupFileName);
                        }
                    }

                    if (backupCreated)
                    {
                        try
                        {
                            File.Delete(tmpBackupFile);
                        }
                        catch (Exception ex)
                        {
                            Error = ex.Message;
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        //RestoreDataFile(DataInterval interval, string symbol, string fileName)
        //BackupAndRemoveDataFile(DataInterval interval, string symbol, string fileName)
    }

}
