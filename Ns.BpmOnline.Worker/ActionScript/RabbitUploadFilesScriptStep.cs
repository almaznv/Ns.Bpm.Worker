using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.IO;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class RabbitUploadFilesScriptStep : ActionScriptStep, IActionScriptStep
    {

        private IRabbitSettings _rabbitSettings;

        public override event ActionScriptStepExitHandler StepExit;
        public override event ActionScriptStepOutputHandler StepOutput;

        private string _ID;
        private string _targetFolder;
        private string _fileType;

        public RabbitUploadFilesScriptStep(ServerElement Server, IRabbitSettings rabbitSettings, 
            Dictionary<string, string> parameters, string fileType) : base(Server)
        {
            _ID = GetByKey(parameters, "ID");
            _rabbitSettings = rabbitSettings;
            _fileType = fileType;
        }

        public void SetTargetFolder(string path)
        {
            _targetFolder = path;
        }

        public override void DoStep()
        {
            try
            {
                SendFiles();
            } catch (Exception e)
            {
                StepExit(-1, "Error in send files " + e.Message + e.StackTrace);
            }
        }

        private void SendFiles()
        {
            DirectoryInfo d = new DirectoryInfo(@_targetFolder);
            FileInfo[] Files = d.GetFiles("*.gz");
            foreach (FileInfo fileInfo in Files)
            {

                byte[] file = File.ReadAllBytes(fileInfo.FullName);
                string logInfo = String.Format("Send file: {0} ({1} Kb)", fileInfo.Name, (fileInfo.Length / 1024).ToString());

                StepOutput(logInfo);
                Logger.Log(logInfo);

                SendFile(fileInfo.Name, file);

                StepOutput("File sent");

            }

            StepExit(0, "File sent end");
        }

        private void SendFile(string fileName, byte[] data)
        {
            IConnection connection = RabbitConnector.GetConnection();

            IModel model = connection.CreateModel();
            model.ExchangeDeclare(_rabbitSettings.ExchangeName, ExchangeType.Direct);
            model.QueueDeclare(_rabbitSettings.QueueName, false, false, false, null);
            model.QueueBind(_rabbitSettings.QueueName, _rabbitSettings.ExchangeName, _rabbitSettings.RoutingKey, null);

            IBasicProperties props = model.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>();
            props.Headers.Add("ID", _ID);
            props.Headers.Add("fileName", fileName);
            props.Headers.Add("fileType", _fileType);

            model.BasicPublish(_rabbitSettings.ExchangeName, _rabbitSettings.RoutingKey, props, data);
            model.Close();

        }

        public override string GetName()
        {
            return "SendFile";
        }
    }
}
