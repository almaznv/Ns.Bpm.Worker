using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class GetFilesScript : ActionScript, IActionScript
    {
        public GetFilesScript(Dictionary<string, string> parameters) : base(parameters)
        {
            List<IActionScriptStep> steps = new List<IActionScriptStep>();

            GetFilesScriptStep scriptStep = new GetFilesScriptStep(parameters);
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
