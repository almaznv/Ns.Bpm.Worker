using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{


     public class BuildFromPackagesScript : ActionScript, IActionScript
     {

         public BuildFromPackagesScript(ServerElement Server, Dictionary<string, string> parameters) : base(Server,
             parameters)
         {
             UpdateFilesRabbitSettings updateFilesRabbitSettings = new UpdateFilesRabbitSettings();
             WorkingPaths workingPaths = new WorkingPaths(Server.Name);

             SavePackagesScriptStep savePackagesStep = new SavePackagesScriptStep(Server, parameters);

             WCUploadToServerScriptStep uploadToServerScriptStep = new WCUploadToServerScriptStep(Server);
             WCBuildConfigurationScriptStep buildConfigurationScriptStep = new WCBuildConfigurationScriptStep(Server);

             List<IActionScriptStep> steps = new List<IActionScriptStep>();

             ClearDirectory(workingPaths.DownloadPackagesPath);
             steps.Add(savePackagesStep);

             bool needBackup = String.IsNullOrEmpty(GetByKey(parameters, "Backup")) ? false : true;
             if (needBackup)
             {
                 WCDownloadFromServerScriptStep downloadFromServerScriptStep =
                     new WCDownloadFromServerScriptStep(Server, parameters);
                 steps.Add(downloadFromServerScriptStep);

                 RabbitUploadFilesScriptStep rabbitUploadFilesScriptStep =
                     new RabbitUploadFilesScriptStep(Server, updateFilesRabbitSettings, parameters, "Backup");
                 rabbitUploadFilesScriptStep.SetTargetFolder(workingPaths.DownloadServerPackagesPath);
                 steps.Add(rabbitUploadFilesScriptStep);
             }

             /*var settings = new UpdateFilesRabbitSettings();
             RabbitUploadBpmPackagesScriptStep uploadBpmPackagesScriptStep = new RabbitUploadBpmPackagesScriptStep(Server, settings, parameters, "Package");
             uploadBpmPackagesScriptStep.SetTargetFolder(workingPaths.DownloadPackagesPath);
             uploadBpmPackagesScriptStep.SetSvnPackagesFolder(workingPaths.DownloadSvnPackagesPath);
             steps.Add(uploadBpmPackagesScriptStep);*/

            steps.Add(uploadToServerScriptStep);
            steps.Add(buildConfigurationScriptStep);

            bool isClearRedis = String.IsNullOrEmpty(GetByKey(parameters, "IsClearRedis")) ? false : true;
            if (isClearRedis)
            {
                var redisHost = GetByKey(parameters, "RedisHost");
                var redisDB = GetByKey(parameters, "RedisDB");
                ClearRedisScriptStep clearRedisScriptStep = new ClearRedisScriptStep(Server, redisHost, redisDB);
                steps.Add(clearRedisScriptStep);

            }

            SetScriptSteps(steps);

            ClearDirectory(workingPaths.DownloadServerPackagesPath);
            //ClearDirectory(workingPaths.DownloadPackagesPath);
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
