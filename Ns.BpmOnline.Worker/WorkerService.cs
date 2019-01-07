using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Configuration;
using System.Net;
using System.IO;
using System.Threading;

namespace Ns.BpmOnline.Worker
{
    

    public partial class WorkerService : ServiceBase
    {
        private IConnection connection;
        private List<IRabbitConsumer> _consumers;
        private ServerElement targetBpmServer;

        public WorkerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            BpmServersConfigSection section = (BpmServersConfigSection)cfg.Sections["BpmServers"];
            targetBpmServer = section.ServerItems[0];

            connection = RabbitConnector.GetConnection();
            RegisterConsumers();

        }


        private void RegisterConsumers()
        {
            _consumers = new List<IRabbitConsumer>()
            {
                new CommandConsumer(connection, new BpmProcessExecutor(targetBpmServer),
                                        targetBpmServer.Name, "PROCESS_EXECUTOR", "PROCESS_EXECUTOR"),
                new CommandConsumer(connection, new BpmServiceExecutor(targetBpmServer),
                                        targetBpmServer.Name, "SERVICE_EXECUTOR", "SERVICE_EXECUTOR")
            };
        }

        protected override void OnStop()
        {
            foreach (var consumer in _consumers)
            {
                consumer.Close();
            }
            connection.Close();
        }
    }
}
