using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{

    public class WCBuildConfigurationScriptStep : RunCmdScriptStep, IActionScriptStep
    {
        //BuildConfiguration
        public WCBuildConfigurationScriptStep(ServerElement Server) : base(Server)
        {

            BpmPaths bpmPaths = new BpmPaths(Server.Path);
            WorkingPaths workingPaths = new WorkingPaths(Server.Name);

            var buildConfigurationSettings = new BuildConfigurationSettings()
            {
                WCpath = bpmPaths.WorkspaceConsoleExePath,
                WebAppPath = bpmPaths.BpmWebAppPath,
                AppPath = bpmPaths.AppPath,
                LogPath = workingPaths.LogsPath
            };

            SetWorkingDirectory(bpmPaths.BpmWebAppPath);
            SetCmdCommand(buildConfigurationSettings.GetCmdParametersStr());
        }

        public override string GetName()
        {
            return "BuildStatic";
        }
    }
}
