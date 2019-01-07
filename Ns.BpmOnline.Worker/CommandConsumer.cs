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
    public class CommandConsumer: RabbitConsumer, IRabbitConsumer
    {

        public CommandConsumer(IConnection connectionInstance, IExecutor commandExecutorInstance, 
                                        string exchangeName, string queueName, string routingKey) : base(connectionInstance)
        {
            executor = commandExecutorInstance;
        
            Register(exchangeName, queueName, routingKey);
        }
    }
}

