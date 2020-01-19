using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;

namespace Ns.BpmOnline.Worker.Executors
{

    public class BpmServiceExecutor : Executor, IExecutor
    {

        public BpmServiceExecutor(string workerName) : base(workerName) { }

        public void Execute(byte[] data, Dictionary<string, object> headers)
        {
            Dictionary<string, string> serviceParameters = DecodeParameters(data);
            Execute(serviceParameters);
        }

        public void Execute(Dictionary<string, string> serviceParameters)
        {
            string serviceName = GetByKey(serviceParameters, "ServiceName");
            /*
            var bpmConnector = new BpmConnector(server.Host);
            bpmConnector.TryLogin(server.Login, server.Password);
            bpmConnector.RunService("POST", serviceName, serviceParameters);*/
        }
    }
}
