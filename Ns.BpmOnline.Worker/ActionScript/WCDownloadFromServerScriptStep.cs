using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;
using System.IO;

namespace Ns.BpmOnline.Worker.ActionScript
{

    public class WCDownloadFromServerScriptStep : RunCmdScriptStep, IActionScriptStep
    {
        private string _workingPath;
        private string _packages;

        //SaveDBContent
        public WCDownloadFromServerScriptStep(ServerElement Server, Dictionary<string, string> parameters) : base(Server)
        {

            BpmPaths bpmPaths = new BpmPaths(Server.Path);
            WorkingPaths workingPaths = new WorkingPaths(Server.Name);

            var downloadPackagesFromServer = new DownloadPackagesFromServerSettings()
            {
                WCpath = bpmPaths.WorkspaceConsoleExePath,
                AppPath = bpmPaths.AppPath,
                LogPath = workingPaths.LogsPath,
                DestinationPath = workingPaths.DownloadServerPackagesPath,
                PackageName = GetByKey(parameters, "Packages")
            };

            _workingPath = workingPaths.DownloadServerPackagesPath;
            _packages = GetByKey(parameters, "Packages");

            SetWorkingDirectory(bpmPaths.BpmWebAppPath);
            SetCmdCommand(downloadPackagesFromServer.GetCmdParametersStr());

            base.StepExit += StepDownloadExit;
        }

        public void RemovePackagesAfterSaveDBContent()
        {
            string[] filePaths = Directory.GetFiles(_workingPath);
            var packagesList = _packages.Split(',');

            foreach (string filePath in filePaths)
            {
                var name = Path.GetFileNameWithoutExtension(new FileInfo(filePath).Name);
                if (!packagesList.Contains(name))
                {
                    File.Delete(filePath);
                }

            }
        }

        public void StepDownloadExit(int exitCode, string message)
        {
            RemovePackagesAfterSaveDBContent();
        }


        public override string GetName()
        {
            return "DownloadPackagesFromServer";
        }
    }
}
