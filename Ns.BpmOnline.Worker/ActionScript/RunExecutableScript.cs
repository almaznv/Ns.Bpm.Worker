using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class RunExecutableScript : ActionScript, IActionScript
    {
        public RunExecutableScript(ServerElement Server, Dictionary<string, string> parameters) : base(Server,
            parameters)
        {
            List<IActionScriptStep> steps = new List<IActionScriptStep>();

            var cmd = GetByKey(parameters, "Cmd");

            RunExecutableScriptStep scriptStep = new RunExecutableScriptStep(Server, cmd);
            steps.Add(scriptStep);

            SetScriptSteps(steps);
        }

        protected override bool ValidateParameters(Dictionary<string, string> parameters)
        {
            string cmd = GetByKey(parameters, "Cmd");
           

            if (String.IsNullOrEmpty(cmd))
            {
                string validateErrorText = String.Format("{0}. Failed execution. Parameter 'Cmd' must be defined", this.GetType().Name);
                SetValidateErrorText(validateErrorText);
                return false;
            }

            return true;
        }
    }
}
