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

            targetBpmServer = Config.GetBpmServer("TargetHost");

            connection = RabbitConnector.GetConnection();
            RegisterConsumers();

        }


        private void RegisterConsumers()
        {
            _consumers = new List<IRabbitConsumer>()
            {
                new CommandConsumer(connection, new BpmProcessExecutor(targetBpmServer), new ProcessExecutorRabbitSettings()),
                new CommandConsumer(connection, new BpmServiceExecutor(targetBpmServer), new ServiceExecutorRabbitSettings()),
                new CommandConsumer(connection, new BpmConfigurationUpdateExecutor(targetBpmServer, connection), new UpdateExecutorRabbitSettings())
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
