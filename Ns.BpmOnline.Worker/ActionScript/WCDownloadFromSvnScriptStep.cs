using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class WCDownloadFromSvnScriptStep : RunCmdScriptStep, IActionScriptStep
    {

        public WCDownloadFromSvnScriptStep(ServerElement Server, Dictionary<string, string> parameters) : base(Server)
        {
            
            BpmPaths bpmPaths = new BpmPaths(Server.Path);
            WorkingPaths workingPaths = new WorkingPaths(Server.Name);

            var svnSettings = new ProjectSvnSettings()
            {
                Uri = _server.SvnUri,
                Login = _server.SvnLogin,
                Password = _server.SvnPassword
            };

            var downloadFromSvnSettings = new DownloadPackagesFromSvnSettings()
            {
                Svn = svnSettings,
                WCpath = bpmPaths.WorkspaceConsoleExePath,
                WorkingPath = workingPaths.DownloadWorkingPath,
                PackagesPath = workingPaths.DownloadPackagesPath,
                LogPath = workingPaths.LogsPath,
                PackageName = GetByKey(parameters, "Packages"),
                Branch = GetByKey(parameters, "Branch")
            };

            SetWorkingDirectory(bpmPaths.BpmWebAppPath);
            SetCmdCommand(downloadFromSvnSettings.GetCmdParametersStr());
        }

        public override string GetName()
        {
            return "DownloadPackagesFromSvn";
        }

    }
}
