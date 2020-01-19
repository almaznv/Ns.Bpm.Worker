using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using RabbitMQ.Client;
using Ns.BpmOnline.Worker.Executors;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{

    public partial class WorkerService : ServiceBase
    {

        private List<IRabbitConsumer> _consumers;

        public WorkerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            RegisterConsumers();
        }


        private void RegisterConsumers()
        {
            string workerName = ConfigurationManager.AppSettings["workerName"];
            IConnection connection = RabbitConnector.GetConnection();
            _consumers = new List<IRabbitConsumer>()
            {
                //new CommandConsumer(connection, new BpmProcessExecutor(targetBpmServer), new ProcessExecutorRabbitSettings()),
                //new CommandConsumer(connection, new BpmServiceExecutor(targetBpmServer), new ServiceExecutorRabbitSettings()),
                new CommandConsumer(connection, new UpdateFilesExecutor(workerName), new UpdateFilesRabbitSettings()),
                new CommandConsumer(connection, new UpdateExecutor(workerName), new UpdateExecutorRabbitSettings())
            };
        }

        protected override void OnStop()
        {
            IConnection connection = RabbitConnector.GetConnection();
            foreach (var consumer in _consumers)
            {
                consumer.Close();
            }
            connection.Close();
        }
    }
}
