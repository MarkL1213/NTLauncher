using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NinjaTraderLauncher
{
    public class StartupWorkspace
    {
        public string WorkspaceName { get; set; }
    }

    public class WorkspaceFile
    {
        public string ConfigFileName { get; set; }
        public string FilePath { get; set; }

        public const string NinjaTraderExecutable = "C:\\Program Files\\NinjaTrader 8\\bin\\NinjaTrader.exe";

        public bool LaunchNinjaTrader()
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = NinjaTraderExecutable;
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
}
