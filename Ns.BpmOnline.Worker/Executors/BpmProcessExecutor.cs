using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Ns.BpmOnline.Worker.Executors
{

    public class BpmProcessExecutor : Executor, IExecutor
    {

        public BpmProcessExecutor(string workerName) : base(workerName) { }

        public void Execute(byte[] data, Dictionary<string, object> headers)
        {
            Dictionary<string, string> processParameters = DecodeParameters(data);
            Execute(processParameters);
        }

        public void Execute(Dictionary<string, string> processParameters)
        {
            string processName = GetByKey(processParameters, "ProcessName");

            /*var bpmConnector = new BpmConnector(server.Host);
            bpmConnector.TryLogin(server.Login, server.Password);
            bpmConnector.RunProcess(processName, processParameters);*/
        }
    }
}
