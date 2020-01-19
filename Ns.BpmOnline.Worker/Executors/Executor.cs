using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ns.BpmOnline.Worker.Executors
{
    public class ExecuteParameter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ExecuteParameters
    {
        public IList<ExecuteParameter> Parameters { get; set; }
    }

    public abstract class Executor
    {
        protected string WorkerName { get; }

        public Executor(string workerName)
        {
            WorkerName = workerName;
        }

        public virtual Dictionary<string, string> DecodeParameters(byte[] data, string format = "JSON")
        {
            var parameters = Encoding.UTF8.GetString(data);

            //Logger.Log(parametersJson);
            switch (format)
            {
                default:
                    return DecodeJSONParameters(parameters);
            }

            //Logger.Log("Parsing service parameters failed : " + e.Message);
        }

        public string GetByKey(Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            } else
            {
                return String.Empty;
            }
        }

        private static Dictionary<string, string>  DecodeJSONParameters(string parametersJson)
        {
            var serviceParameters = new Dictionary<string, string>();
            ExecuteParameters desirializedParameters = JsonConvert.DeserializeObject<ExecuteParameters>(parametersJson);

            foreach (ExecuteParameter param in desirializedParameters.Parameters)
            {
                serviceParameters.Add(param.Key, param.Value);
            }
            return serviceParameters;
        }
    }
}
