using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public delegate void ActionScriptExitHandler(int exitCode, string message);
    public delegate void ActionScriptActionHandler(string action, bool success, string comment);
    public delegate void ActionScriptOutputHandler(string message);
    

    public interface IActionScript
    {
        event ActionScriptExitHandler ScriptExit;
        event ActionScriptOutputHandler ScriptOutput;
        event ActionScriptActionHandler ScriptAction;

        void Run();
    }

    public abstract class ActionScript
    {
        private List<IActionScriptStep> _scriptSteps = new List<IActionScriptStep>();
        private int _currentStepIndex = 0;
        IActionScriptStep _currentStep;
        Dictionary<string, string> _inputParameters;

        string _validateErrorText;

        public bool IsScriptSucceed { get; private set; }

        public virtual event ActionScriptExitHandler ScriptExit;
        public virtual event ActionScriptOutputHandler ScriptOutput;
        public virtual event ActionScriptActionHandler ScriptAction;

        public ActionScript(Dictionary<string, string> parameters)
        {
            _inputParameters = parameters;
            IsScriptSucceed = true;
        }

        public virtual void Run()
        {
            if (_scriptSteps.Count == 0)
            {
                ScriptExit(-1, "Script steps not found");
                return;
            };

            if (ValidateParameters(_inputParameters) == true)
            {
                RunNext(_currentStepIndex);
            }
            else
            {
                ScriptExit(-1, _validateErrorText);
                IsScriptSucceed = false;
            }

           
        }

        private void RunNext(int stepIndex)
        {

            _currentStep = _scriptSteps[stepIndex];

            ScriptAction("STARTSTEP", true, _currentStep.GetName());

            _currentStep.StepOutput += StepOutput;
            _currentStep.StepExit += StepExit;
            _currentStep.DoStep();
        }

        protected void SetScriptSteps(List<IActionScriptStep> steps)
        {
            _scriptSteps = steps;
        }

        private void StepOutput(string message)
        {
            ScriptOutput(message);
        }

        private void StepExit(int exitCode, string message)
        {
            ScriptAction("FINISHSTEP", ((exitCode < 0) ? false: true), _currentStep.GetName());

            if (exitCode < 0)
            {
                ScriptExit(exitCode, message);
                return;
            }

            _currentStepIndex++;
            if (_currentStepIndex < _scriptSteps.Count)
            {
                RunNext(_currentStepIndex);
            } else
            {
                ScriptExit(0, "End of script");
            }
        }

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

        protected virtual bool ValidateParameters(Dictionary<string, string> parameters)
        {
            return true;
        }

        protected void SetValidateErrorText(string text)
        {
            _validateErrorText = text;
        }

        protected void ClearDirectory(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
        }
    }
}
