using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class RunExecutableScriptStep : RunCmdScriptStep, IActionScriptStep
    {
        //InstallFromRepository
        public RunExecutableScriptStep(ServerElement Server, string cmd) : base(Server)
        {

            BpmPaths bpmPaths = new BpmPaths(Server.Path);

            SetWorkingDirectory(bpmPaths.BpmWebAppPath);
            SetCmdCommand(cmd);
        }

        public override string GetName()
        {
            return "RunExecutable";
        }
    }
}
