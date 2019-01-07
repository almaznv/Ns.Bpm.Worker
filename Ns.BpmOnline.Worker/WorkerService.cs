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
        private IConnection _connection;
        private List<IRabbitConsumer> _consumers;

        public WorkerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _connection = RabbitConnector.GetConnection();
            RegisterConsumers();

        }

        private void RegisterConsumers()
        {
            _consumers = new List<IRabbitConsumer>()
            {
                new BpmProcessExecuteConsumer(_connection)
            };
        }

        protected override void OnStop()
        {
            foreach (var consumer in _consumers)
            {
                consumer.Close();
            }
            _connection.Close();
        }
    }
}
