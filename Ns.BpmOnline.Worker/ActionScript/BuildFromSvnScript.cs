using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class BuildFromSvnScript : ActionScript, IActionScript
    {

        public BuildFromSvnScript(ServerElement Server, Dictionary<string, string> parameters) : base(Server, parameters)
        {
            WorkingPaths workingPaths = new WorkingPaths(Server.Name);
            WCDownloadFromSvnScriptStep downloadFromSvnStep = new WCDownloadFromSvnScriptStep(Server, parameters);
            WCUploadToServerScriptStep uploadToServerScriptStep  = new WCUploadToServerScriptStep(Server);
            WCBuildConfigurationScriptStep buildConfigurationScriptStep = new WCBuildConfigurationScriptStep(Server);

            List<IActionScriptStep> steps = new List<IActionScriptStep>();

            bool needBackup = String.IsNullOrEmpty(GetByKey(parameters, "Backup")) ? false : true;
            if (needBackup)
            {
                WCDownloadFromServerScriptStep downloadFromServerScriptStep = new WCDownloadFromServerScriptStep(Server, parameters);
                steps.Add(downloadFromServerScriptStep);
            }

            steps.Add(downloadFromSvnStep);

            var settings = new UpdateFilesRabbitSettings();
            RabbitUploadBpmPackagesScriptStep uploadBpmPackagesScriptStep = new RabbitUploadBpmPackagesScriptStep(Server, settings, parameters, "PackagesForInstall");
            uploadBpmPackagesScriptStep.SetTargetFolder(workingPaths.DownloadPackagesPath);
            uploadBpmPackagesScriptStep.SetSvnPackagesFolder(workingPaths.DownloadSvnPackagesPath);
            steps.Add(uploadBpmPackagesScriptStep);


            steps.Add(uploadToServerScriptStep);
            steps.Add(buildConfigurationScriptStep);

            SetScriptSteps(steps);

           
            ClearDirectory(workingPaths.DownloadPackagesPath);
            ClearDirectory(workingPaths.TempUploadDirectory);
        }

        protected override bool ValidateParameters(Dictionary<string, string> parameters)
        {
            string packages = GetByKey(parameters, "Packages");
            string branch = GetByKey(parameters, "Branch");

            if (String.IsNullOrEmpty(packages))
            {
                string validateErrorText = String.Format("{0}. Failed execution. Parameter 'Packages' must be defined", this.GetType().Name);
                SetValidateErrorText(validateErrorText);
                return false;
            }
            if (String.IsNullOrEmpty(branch))
            {
                string validateErrorText = String.Format("{0}. Failed execution. Parameter 'Branch' must be defined", this.GetType().Name);
                SetValidateErrorText(validateErrorText);
                return false;
            }

            return true;
        }
    }
}
