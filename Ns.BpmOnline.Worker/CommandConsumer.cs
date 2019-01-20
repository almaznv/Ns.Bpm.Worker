using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Configuration;
using Ns.BpmOnline.Worker.Executors;

namespace Ns.BpmOnline.Worker
{
    public class CommandConsumer: RabbitConsumer, IRabbitConsumer
    {

        public CommandConsumer(IConnection connectionInstance, IExecutor commandExecutorInstance, IRabbitSettings rabbitSettings) : base(connectionInstance)
        {
            executor = commandExecutorInstance;

            Register(rabbitSettings.ExchangeName, rabbitSettings.QueueName, rabbitSettings.QueueName);
        }
    }
}

