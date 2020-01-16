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
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (ea.BasicProperties.Headers != null)
            {
                foreach (string headerKey in ea.BasicProperties.Headers.Keys)
                {
                    byte[] value = ea.BasicProperties.Headers[headerKey] as byte[];
                    dict.Add(headerKey, Encoding.UTF8.GetString(value));

                }
            }

            executor.Execute(ea.Body, dict);
        }

        protected virtual IModel GetRabbitChannel(string exchangeName, string queueName, string routingKey)
        {
            IModel model = connection.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("x-expires", 1800000);
            model.QueueDeclare(queueName, false, false, true, args);
            model.QueueBind(queueName, exchangeName, routingKey, null);
            return model;
        }

    }
}
