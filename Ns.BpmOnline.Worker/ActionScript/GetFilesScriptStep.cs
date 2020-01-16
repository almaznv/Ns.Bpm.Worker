using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Ns.BpmOnline.Worker.Parameters;
using RabbitMQ.Client;

namespace Ns.BpmOnline.Worker.ActionScript
{
    public class GetFilesScriptStep : ActionScriptStep, IActionScriptStep
    {
        public override event ActionScriptStepExitHandler StepExit;
        public override event ActionScriptStepOutputHandler StepOutput;

        private string WorkingDirectory { get; set; }
        private Dictionary<string, string> Parameters { get; set; }

        public GetFilesScriptStep(ServerElement Server, Dictionary<string, string> parameters) : base(Server)
        {
            WorkingDirectory = GetByKey(parameters, "Cmd");
            Parameters = parameters;
        }

        private void SendFile(string fileName, string fileSize, byte[] data, string exchangeName, string queueName, bool isFinish)
        {
            if (exchangeName == string.Empty || queueName == string.Empty) return;

            IConnection connection = RabbitConnector.GetConnection();

            var parameters = new Dictionary<string, object>() { };

            parameters.Add("Type", "File");
            parameters.Add("FileName", fileName);
            parameters.Add("FileSize", fileSize);
            parameters.Add("Destination", GetByKey(Parameters, "Cmd"));
            parameters.Add("IsFinish", isFinish.ToString());

            parameters.Add("UniqueId", GetByKey(Parameters, "UniqueId"));
            parameters.Add("BuildKeyId", GetByKey(Parameters, "BuildKeyId"));
            parameters.Add("ID", GetByKey(Parameters, "ID"));
            parameters.Add("ReleaseId", GetByKey(Parameters, "ReleaseId"));
            if (Parameters.ContainsKey("Variables"))
            {
                parameters.Add("Variables", GetByKey(Parameters, "Variables"));
            }

            RabbitPublisher.PublishFile(connection, exchangeName, queueName, queueName, parameters, data);


        }

        public override void DoStep()
        {
            string exchangeName = GetByKey(Parameters, "RabbitExchangeName");
            string queueName = GetByKey(Parameters, "RabbitQueueName");

            SendFiles(exchangeName, queueName);

            StepExit(0, "Send files end");
        }

        private void SendFiles(string exchangeName, string queueName)
        {
            string ext = GetByKey(Parameters, "Extension") ?? "";
            ext = (ext == String.Empty) ? "*.*" : ext;
            DirectoryInfo d = new DirectoryInfo(WorkingDirectory);
            var files = d.GetFiles(ext);
            int filesCount = files.Length;
            int i = 0;
            foreach (var fileInfo in files)
            {
                bool isFinish = (filesCount == (i + 1));
                byte[] file = File.ReadAllBytes(fileInfo.FullName);
                string fileSize = (fileInfo.Length / 1024).ToString();

                SendFile(fileInfo.Name, fileSize, file, exchangeName, queueName, isFinish);
                string logInfo = String.Format("Send file: {0} ({1}) to {2} {3}", fileInfo.Name, fileSize, exchangeName, queueName);
                StepOutput(logInfo);
                Logger.Log(logInfo);
                i++;
            }
        }

        public override string GetName()
        {
            return "GetFiles";
        }

    }
}