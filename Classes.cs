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
        public string FilePath { get; set; }

        public string SetStartupWorkspace(StartupWorkspace workspace)
        {
            if (!File.Exists(FilePath))
            {
                return "File does not exist";
            }

            try
            {
                string[] lines = File.ReadAllLines(FilePath);
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

                File.WriteAllLines(FilePath, lines);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return string.Empty;
        }

        public string LookupCurrentWorkspace()
        {
            if (!File.Exists(FilePath))
            {
                return string.Empty;
            }
            try
            {
                string[] lines = File.ReadAllLines(FilePath);
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
