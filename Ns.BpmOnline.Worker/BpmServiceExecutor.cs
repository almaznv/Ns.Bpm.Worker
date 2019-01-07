using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;

namespace Ns.BpmOnline.Worker
{

    public class BpmServiceExecutor : IExecutor
    {
        private ServerElement _server;

        public BpmServiceExecutor(ServerElement server)
        {
            _server = server;
        }

        public void Execute(byte[] data)
        {
            var serviceParameters = new Dictionary<string, string>() { };
            var parametersJson = Encoding.UTF8.GetString(data);

            try
            {
                ExecuteParameters desirializedParameters = JsonConvert.DeserializeObject<ExecuteParameters>(parametersJson);

                foreach (ExecuteParameter param in desirializedParameters.Parameters)
                {
                    serviceParameters.Add(param.Key, param.Value);
                }
            }
            catch (Exception e)
            {
                Logger.Log("Parsing service parameters failed : " + e.Message);
                return;
            }

            var message = Encoding.UTF8.GetString(data);

            var bpmConnector = new BpmConnector(_server.Host);
            bpmConnector.TryLogin(_server.Login, _server.Password);
            bpmConnector.RunService("GET", "NsTestService/Test", serviceParameters);
        }
    }
}
