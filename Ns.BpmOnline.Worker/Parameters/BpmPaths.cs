using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ns.BpmOnline.Worker.Parameters
{
    public class BpmPaths
    {
        public readonly string AppPath;

        public string BpmWebAppPath => Path.Combine(AppPath, "Terrasoft.WebApp");
        public string WorkspaceConsoleDirectoryPath => Path.Combine(BpmWebAppPath, "DesktopBin", "WorkspaceConsole");
        public string WorkspaceConsoleExePath => Path.Combine(WorkspaceConsoleDirectoryPath, "Terrasoft.Tools.WorkspaceConsole.exe");
        public BpmPaths(string appPath)
        {
            AppPath = appPath;
        }
        
    }
}
