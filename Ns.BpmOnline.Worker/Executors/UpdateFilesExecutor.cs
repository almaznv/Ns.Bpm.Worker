using System;
using System.Collections.Generic;
using System.IO;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace Ns.BpmOnline.Worker.Executors
{

    public class UpdateFilesExecutor : Executor, IExecutor
    {
        public UpdateFilesExecutor(string workerName) : base(workerName) { }

        public void Execute(byte[] data, Dictionary<string, object> headers)
        {
            List<string> outputList = new List<string>();
            List<BpmConfigurationUpdateStatus> actionList = new List<BpmConfigurationUpdateStatus>();

            Guid StepId = (headers.ContainsKey("ID") ? Guid.Parse((string)headers["ID"]) : Guid.Empty);
            string FileName = (headers.ContainsKey("FileName") ? (string)headers["FileName"] : String.Empty);
            string Destination = (headers.ContainsKey("Destination") ? (string)headers["Destination"] : String.Empty);

            int FileSize = 0;
            if (headers.ContainsKey("FileSize"))
            {
                Int32.TryParse((string)headers["FileSize"], out FileSize);
            }

            bool IsFinish = false;
            if (headers.ContainsKey("IsFinish"))
            {
                Boolean.TryParse((string)headers["IsFinish"], out IsFinish);
            }

            //Guid FileId = (headers.ContainsKey("FileId") ? Guid.Parse((string)headers["FileId"]) : Guid.Empty);


            ScriptOutput(outputList, String.Format("Getting file {0} ({1}), to put in: {2}", FileName, FileSize.ToString(), Destination));

            SaveFile((string)FileName, (string)Destination, data);

            ScriptOutput(outputList, "Saved");

            if (IsFinish)
            {
                //SendAnswer(StepId, FileId);
                ScriptOutput(outputList, String.Format("Ack step {0}. Is finish", StepId.ToString()));
                LogStepAction(actionList, "FINISH", true, String.Empty);
            }

            
            SendResponse(StepId, outputList, actionList);
        }

        private bool SaveFile(string fileName, string destination, byte[] data)
        {
            string resultingFileName = Path.Combine(destination, fileName);

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            File.WriteAllBytes(resultingFileName, data);

            return true;
        }

        /*
        private void SendAnswer(Guid stepId, Guid fileId)
        {
            var rSettings = new UpdateFilesRabbitSettings(server.Name);

            IConnection connection = RabbitConnector.GetConnection();

            var parameters = new Dictionary<string, object>() { };

            parameters.Add("Type", "Ack");
            parameters.Add("ID", stepId.ToString());
            parameters.Add("FileId", fileId.ToString());

            RabbitPublisher.PublishFile(connection, rSettings.ExchangeName, rSettings.AnswerQueueName, rSettings.AnswerRoutingKey, parameters, new byte[]{});
        }*/


        private void ScriptOutput(List<string> outputList, string message)
        {
            outputList.Add(message);
            Logger.Log(message);
        }

        private void LogStepAction(List<BpmConfigurationUpdateStatus> actionList, string action, bool success, string comment)
        {
            if (!success)
            {
                comment += " finished with errors";
            }

            string statusStr = (success) ? "Succeed" : "Unsucceed";
            var status = new BpmConfigurationUpdateStatus()
            {
                Timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"),
                Name = action,
                Success = success,
                Comment = comment
            };
            Logger.Log(String.Format("[{0} {1}]: {2}, {3}", status.Timestamp, status.Name, statusStr, status.Comment));

            actionList.Add(status);
        }

        private void SendResponse(Guid StepId, List<string> outputList, List<BpmConfigurationUpdateStatus> actionList)
        {
            var rSettings = new UpdateExecutorRabbitSettings(WorkerName);

            Logger.Log(String.Format("[{0} {1}]", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), "SendBuildResponse"));

            var response = new BpmConfigurationUpdateResponse()
            {
                OutputList = outputList,
                ActionList = actionList,
                Id = StepId.ToString()
            };
            string message = JsonConvert.SerializeObject(response);
            IConnection connection = RabbitConnector.GetConnection();

            RabbitPublisher.Publish(connection, rSettings.ExchangeName, rSettings.AnswerQueueName, rSettings.AnswerRoutingKey, message);
        }
    }
}
