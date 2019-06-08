

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class SavePackagesScriptStep : ActionScriptStep, IActionScriptStep
    {
        public override event ActionScriptStepExitHandler StepExit;
        public override event ActionScriptStepOutputHandler StepOutput;

        private string _workingDirectory;
        private Dictionary<string, string> _parameters;

        public SavePackagesScriptStep(ServerElement Server, Dictionary<string, string> parameters) : base(Server)
        {
            BpmPaths bpmPaths = new BpmPaths(Server.Path);
            WorkingPaths workingPaths = new WorkingPaths(Server.Name);

            _workingDirectory = workingPaths.DownloadPackagesPath;
            _parameters = parameters;

        }

        public override void DoStep()
        {
            var packages = GetByKey(_parameters, "Packages");
            foreach (var packageName in packages.Split(','))
            {
                var packageData = GetByKey(_parameters, "PackagesData_" + packageName);
                var packageExt = GetByKey(_parameters, "PackagesDataExt_" + packageName);
                packageExt = (packageExt == String.Empty) ? ".gz" : packageExt;
                byte[] data = Convert.FromBase64String(packageData);
                string fileName = _workingDirectory + "\\" + packageName + packageExt;
                File.WriteAllBytes(fileName, data);
            }
            StepOutput("Files saved");
            StepExit(1, String.Empty);
        }

        public override string GetName()
        {
            return "SavePackages";
        }

    }
}