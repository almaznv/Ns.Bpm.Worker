using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{
    public class BpmProcessExecuteConsumer: RabbitConsumer, IRabbitConsumer
    {

        private const string EXCHANGE_NAME = "Localhost__Test";
        private const string QUEUE_NAME = "PROCESS_EXECUTOR";
        private const string ROUTING_KEY = "PROCESS_EXECUTOR";

        public BpmProcessExecuteConsumer(IConnection connection) : base(connection)
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            BpmServersConfigSection section = (BpmServersConfigSection)cfg.Sections["BpmServers"];
            ServerElement server = section.ServerItems[0];

            _executor = new BpmProcessExecutor(server);
        
            Register(EXCHANGE_NAME, QUEUE_NAME, ROUTING_KEY);
        }
    }
}

