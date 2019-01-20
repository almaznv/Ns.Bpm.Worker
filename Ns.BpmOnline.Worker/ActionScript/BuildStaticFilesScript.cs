using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class BuildStaticFilesScript : ActionScript, IActionScript
    {
        public BuildStaticFilesScript(ServerElement Server, Dictionary<string, string> parameters) : base(Server, parameters)
        {
            WorkingPaths workingPaths = new WorkingPaths(Server.Name);
            WCBuildConfigurationScriptStep buildConfigurationScriptStep = new WCBuildConfigurationScriptStep(Server);

            List<IActionScriptStep> steps = new List<IActionScriptStep>();

            steps.Add(buildConfigurationScriptStep);

            SetScriptSteps(steps);

        }
    }

    
}
