using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Ns.BpmOnline.Worker
{
    public interface IExecutor
    {
        void Execute(byte[] data);
    }

    public class ExecuteParameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ExecuteParameters
    {
        public IList<ExecuteParameter> Parameters { get; set; }
    }

    public class BpmProcessExecutor : IExecutor
    {
        private ServerElement _server;

        public BpmProcessExecutor(ServerElement server)
        {
            _server = server;
        }

        public void Execute(byte[] data)
        {
            var processParameters = new Dictionary<string, string>() { };
            var parametersJson = Encoding.UTF8.GetString(data);

            try
            {
                ExecuteParameters desirializedParameters = JsonConvert.DeserializeObject<ExecuteParameters>(parametersJson);

                foreach (ExecuteParameter param in desirializedParameters.Parameters)
                {
                    processParameters.Add(param.Key, param.Value);
                }
            } catch (Exception e)
            {
                Logger.Log("Parsing process parameters failed : " + e.Message);
                return;
            }

            var bpmConnector = new BpmConnector(_server.Host);
            bpmConnector.TryLogin(_server.Login, _server.Password);

            Logger.Log("Run process");

            bpmConnector.RunProcess("MqTestProcess", processParameters);
        }
    }
}
