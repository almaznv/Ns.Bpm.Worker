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
    public interface IRabbitConsumer
    {
        void Register(string exchangeName, string queueName, string routingKey);
        void Close();
    }

    public abstract class RabbitConsumer : IRabbitConsumer
    {
        private IConnection _connection;
        protected IExecutor _executor;
        private IModel channel;

        public RabbitConsumer(IConnection connection)
        {
            _connection = connection;
        }

        public void Register(string exchangeName, string queueName, string routingKey)
        {
            channel = GetRabbitChannel(exchangeName, queueName, routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += onMessage;
            channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);
        }

        public void Close()
        {
            if (channel != null)
            {
                channel.Close();
            }
        }

        public virtual void onMessage(object model, BasicDeliverEventArgs ea)
        {
            _executor.Execute(ea.Body);
        }

        private IModel GetRabbitChannel(string exchangeName, string queueName, string routingKey)
        {
            IModel model = _connection.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            model.QueueDeclare(queueName, false, false, false, null);
            model.QueueBind(queueName, exchangeName, routingKey, null);
            return model;
        }

    }
}
