using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ns.BpmOnline.Worker.ActionScript
{
    public delegate void ActionScriptStepExitHandler(int exitCode, string message);
    public delegate void ActionScriptStepOutputHandler(string message);

    public interface IActionScriptStep
    {
        event ActionScriptStepExitHandler StepExit;
        event ActionScriptStepOutputHandler StepOutput;

        void DoStep();
        string GetName();
    }

    public abstract class ActionScriptStep : IActionScriptStep
    {

        abstract public event ActionScriptStepExitHandler StepExit;
        abstract public event ActionScriptStepOutputHandler StepOutput;

        public ActionScriptStep()
        {

        }

        abstract public void DoStep();
        abstract public string GetName();

        protected string GetByKey(Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                return String.Empty;
            }
        }


    }
}
