using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RabbitMQ.Client;
using Ns.BpmOnline.Worker.ActionScript;
using Newtonsoft.Json;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.Executors
{
    public class UpdateExecutor : Executor, IExecutor
    {

        private readonly int _batchSendTimeout = 5000;
        private readonly UpdateExecutorRabbitSettings _rabbitSettings;
        private Dictionary<string, string> _inputParameters;


        private Timer _batchSendTimer;
        private IActionScript _script;
        private readonly List<string> _outputList = new List<string>();
        private readonly List<BpmConfigurationUpdateStatus> _actionList = new List<BpmConfigurationUpdateStatus>();


        public UpdateExecutor(string workerName) : base(workerName) 
        {
            _rabbitSettings = new UpdateExecutorRabbitSettings();
        }

        public void Execute(byte[] data, Dictionary<string, object> headers)
        {
            try
            {
                Dictionary<string, string> parameters = DecodeParameters(data);
                Execute(parameters);
            }
            catch (Exception e)
            {
                Logger.Log("Error in RunExecutable " + e.Message + e.StackTrace);
            }

            
        }

        private void Execute(Dictionary<string, string> parameters)
        {
            
            if (ValidateParameters(parameters) == false)
            {
                return;
            }

            _inputParameters = parameters;
            InitResponseTimer();

            string command = GetByKey(parameters, "Command");
            Logger.Log("Execute command " + command);

            switch (command)
            {
                case "RunExecutable":
                    RunExecutable(parameters);
                    break;
                case "GetFiles":
                    GetFiles(parameters);
                    break;
                default:
                    StopAndClear();
                    break;
            }
        }

        private void GetFiles(Dictionary<string, string> parameters)
        {
            try
            {
                _script = new GetFilesScript(parameters);

                _script.ScriptOutput += ScriptOutput;
                _script.ScriptExit += ScriptExit;
                _script.ScriptAction += LogStepAction;
                _script.Run();
            }
            catch (Exception e)
            {
                _script = null;
                Logger.Log("Error in GetFiles " + e.Message + e.StackTrace);
            }
        }

        private void RunExecutable(Dictionary<string, string> parameters)
        {
            try
            {
                _script = new RunExecutableScript(parameters);

                _script.ScriptOutput += ScriptOutput;
                _script.ScriptExit += ScriptExit;
                _script.ScriptAction += LogStepAction;
                _script.Run();
            }
            catch (Exception e)
            {
                _script = null;
                Logger.Log("Error in RunExecutable " + e.Message + e.StackTrace);
            }
        }

        private void ScriptOutput(string message)
        {
            _outputList.Add(message);
            //Logger.Log(message);
        }

        private void ScriptExit(int exitCode, string message)
        {
            bool IsScriptSucceed = (exitCode < 0) ? false : true;
            LogStepAction("FINISH", IsScriptSucceed, message);

        }

        private bool ValidateParameters(Dictionary<string, string> parameters)
        {
            string ID = GetByKey(parameters, "ID");
            string command = GetByKey(parameters, "Command");

            if (String.IsNullOrEmpty(ID))
            {
                Logger.Log("Update: Failed execution. Parameter 'ID' must be defined");
                return false;
            }
            if (String.IsNullOrEmpty(command))
            {
                Logger.Log("Update: Failed execution. Parameter 'Command' must be defined");
                return false;
            }

            return true;
        }

        private void LogStepAction(string action, bool success, string comment)
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
            
            _actionList.Add(status);
        }

        private void InitResponseTimer()
        {
            _batchSendTimer = new Timer() { Interval = _batchSendTimeout };
            _batchSendTimer.Elapsed += OnReponseSendTimerEvent;
            _batchSendTimer.Enabled = true;
        }

        private void OnReponseSendTimerEvent(Object source, ElapsedEventArgs e)
        {
            BpmConfigurationUpdateStatus finishState = _actionList.ToArray().FirstOrDefault(x => x.Name == "FINISH");
            if (finishState != null)
            {
                StopAndClear();
            }

            SendResponse(_outputList, _actionList);

            _outputList.Clear();
            _actionList.Clear();
        }

        private void SendResponse(List<string> outputList, List<BpmConfigurationUpdateStatus> actionList)
        {
            Logger.Log(String.Format("[{0} {1}]", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), "SendBuildResponse"));
            var response = new BpmConfigurationUpdateResponse()
            {
                OutputList = outputList,
                ActionList = actionList,
                Id = _inputParameters["ID"]
            };
            string message = JsonConvert.SerializeObject(response);
            IConnection connection = RabbitConnector.GetConnection();

            RabbitPublisher.Publish(connection, _rabbitSettings.ExchangeName, _rabbitSettings.AnswerQueueName, _rabbitSettings.AnswerRoutingKey, message);
        }

        private void StopAndClear()
        {
            _batchSendTimer.Enabled = false;
            _script = null;
        }
    }

    public class BpmConfigurationUpdateStatus
    {
        public string Timestamp { get; set; }
        public string Name { get; set; }
        public bool Success { get; set; }
        public string Comment { get; set; }
    }

    public class BpmConfigurationUpdateResponse
    {
        public List<string> OutputList { get; set; }
        public List<BpmConfigurationUpdateStatus> ActionList { get; set; }
        public string Id { get; set; }
    }
}
