using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RabbitMQ.Client;
using SharpSvn;

namespace Ns.BpmOnline.Worker.ActionScript
{

    public class RabbitUploadBpmPackagesScriptStep : ActionScriptStep, IActionScriptStep
    {
        private IRabbitSettings _rabbitSettings;

        public override event ActionScriptStepExitHandler StepExit;
        public override event ActionScriptStepOutputHandler StepOutput;

        private string _ID;
        private string _packages;
        private string _branch;
        private string _targetFolder;
        private string _svnPackagesFolder;
        private string _fileType;
        private bool _needDownloadPackages = false;

        private Dictionary<string, string> _packagesInfo = new Dictionary<string, string>();

        public RabbitUploadBpmPackagesScriptStep(ServerElement Server, IRabbitSettings rabbitSettings,
            Dictionary<string, string> parameters, string fileType) : base(Server)
        {
            _ID = GetByKey(parameters, "ID");
            _packages = GetByKey(parameters, "Packages");
            _branch = GetByKey(parameters, "Branch");
            _rabbitSettings = rabbitSettings;
            _fileType = fileType;
            _needDownloadPackages = String.IsNullOrEmpty(GetByKey(parameters, "DownloadPackages")) ? false : true;
        }

        public void SetTargetFolder(string path)
        {
            _targetFolder = path;
        }

        public void SetSvnPackagesFolder(string path)
        {
            _svnPackagesFolder = path;
        }

        public override void DoStep()
        {

            try
            {
                CollectPackagesSvnInfo();
                SendFiles();
            }
            catch (Exception e)
            {
                StepExit(-1, "Error in send files " + e.Message + e.StackTrace);
            }
        }

        private void CollectPackagesSvnInfo()
        {
            var workingCopyClient = new SvnWorkingCopyClient();

            foreach (string _packageName in _packages.Split(',') )
            {
                string packageName = _packageName.Trim();
                string svnPackageDirectory = Path.Combine(_svnPackagesFolder, packageName);
                if (!Directory.Exists(svnPackageDirectory)) continue;

                SvnWorkingCopyVersion version;
                workingCopyClient.GetVersion(svnPackageDirectory, out version);

                long localRev = version.End;
                _packagesInfo.Add(packageName + "_REVNUM", localRev.ToString());
                _packagesInfo.Add(packageName + "_BRANCH", _branch);
            }
        }

        private void SendFiles()
        {
            foreach (string _packageName in _packages.Split(','))
            {
                string packageName = _packageName.Trim();

                string filePath = Path.Combine(_targetFolder, packageName + ".gz");
                if (!File.Exists(filePath)) continue;

                FileInfo fileInfo = new FileInfo(@filePath);

                byte[] file = File.ReadAllBytes(fileInfo.FullName);
                string fileSize = (fileInfo.Length / 1024).ToString() + " Kb";
                _packagesInfo.Add(packageName + "_SIZE", fileSize);

                string logInfo;
                if (_needDownloadPackages == true)
                {
                    logInfo = String.Format("Send file: {0} ({1})", fileInfo.Name, fileSize);
                } else
                {
                    logInfo = String.Format("Send file info: {0} ({1})", fileInfo.Name, fileSize);
                }

                StepOutput(logInfo);
                Logger.Log(logInfo);

                SendFile(packageName, fileInfo.Name, file);

                StepOutput("File sent");

            }

            StepExit(0, "Send packages end");
        }

        private void SendFile(string packageName, string fileName, byte[] data)
        {
            IConnection connection = RabbitConnector.GetConnection();

            IModel model = connection.CreateModel();
            model.ExchangeDeclare(_rabbitSettings.ExchangeName, ExchangeType.Direct);
            model.QueueDeclare(_rabbitSettings.QueueName, false, false, false, null);
            model.QueueBind(_rabbitSettings.QueueName, _rabbitSettings.ExchangeName, _rabbitSettings.RoutingKey, null);

            IBasicProperties props = model.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>();
            props.Headers.Add("ID", _ID);
            props.Headers.Add("packageName", packageName);
            props.Headers.Add("fileName", fileName);
            props.Headers.Add("fileType", _fileType);

            if (_packagesInfo.ContainsKey(packageName + "_REVNUM"))
            {
                props.Headers.Add("revNum", _packagesInfo[packageName + "_REVNUM"]);
            }
            if (_packagesInfo.ContainsKey(packageName + "_SIZE"))
            {
                props.Headers.Add("size", _packagesInfo[packageName + "_SIZE"]);
            }
            if (_packagesInfo.ContainsKey(packageName + "_BRANCH"))
            {
                props.Headers.Add("branch", _packagesInfo[packageName + "_BRANCH"]);
            }

            if (_needDownloadPackages == false)
            {
                data = new byte[] { };
            }
            model.BasicPublish(_rabbitSettings.ExchangeName, _rabbitSettings.RoutingKey, props, data);
            model.Close();

        }

        public override string GetName()
        {
            return "SendBpmPackages";
        }
    }
}
