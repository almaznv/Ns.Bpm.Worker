using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class RunExecutableScriptStep : RunCmdScriptStep, IActionScriptStep
    {
        public RunExecutableScriptStep(string cmd) : base()
        {
            SetWorkingDirectory(System.IO.Directory.GetCurrentDirectory());
            SetCmdCommand(cmd);
        }

        public override string GetName()
        {
            return "RunExecutable";
        }
    }
}
