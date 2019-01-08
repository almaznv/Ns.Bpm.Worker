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
        protected IConnection connection;
        protected IExecutor executor;
        private IModel channel;

        public RabbitConsumer(IConnection _connection)
        {
            connection = _connection;
        }

        public virtual void Register(string exchangeName, string queueName, string routingKey)
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
            executor.Execute(ea.Body);
        }

        protected virtual IModel GetRabbitChannel(string exchangeName, string queueName, string routingKey)
        {
            IModel model = connection.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            model.QueueDeclare(queueName, false, false, false, null);
            model.QueueBind(queueName, exchangeName, routingKey, null);
            return model;
        }

    }
}
