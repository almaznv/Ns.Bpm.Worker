using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{

    public class WCUploadToServerScriptStep : RunCmdScriptStep, IActionScriptStep
    {
        //InstallFromRepository
        public WCUploadToServerScriptStep(ServerElement Server) : base(Server)
        {

            BpmPaths bpmPaths = new BpmPaths(Server.Path);
            WorkingPaths workingPaths = new WorkingPaths(Server.Name);

            var uploadToServerSettings = new UploadPackagesToServerSettings()
            {
                WCpath = bpmPaths.WorkspaceConsoleExePath,
                PackagesPath = workingPaths.DownloadPackagesPath,
                WorkingTempPath = workingPaths.TempUploadDirectory,
                WebAppPath = bpmPaths.BpmWebAppPath,
                AppPath = bpmPaths.AppPath,
                LogPath = workingPaths.LogsPath
            };

            SetWorkingDirectory(bpmPaths.BpmWebAppPath);
            SetCmdCommand(uploadToServerSettings.GetCmdParametersStr());
        }

        public override string GetName()
        {
            return "UploadToServer";
        }
    }
}
