using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ns.BpmOnline.Worker
{
    static class RabbitPublisher
    {

        public static void Publish(IConnection Connection, string ExchangeName, string QueueName, string RoutingKey, string Message)
        {
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(Message);
            Publish(Connection, ExchangeName, QueueName, RoutingKey, messageBodyBytes);
        }

        public static void Publish(IConnection Connection, string ExchangeName, string QueueName, string RoutingKey, byte[] messageBodyBytes)
        {
            IModel channel = GetRabbitChannel(Connection, ExchangeName, QueueName, RoutingKey);
            channel.BasicPublish(ExchangeName, RoutingKey, null, messageBodyBytes);
            channel.Close();
        }

        public static void PublishFile(IConnection Connection, string ExchangeName, string QueueName, string RoutingKey, Dictionary<string, object> parameters, byte[] data)
        {
            IModel channel = GetRabbitChannel(Connection, ExchangeName, QueueName, RoutingKey);

            IBasicProperties props = channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>();
            foreach (var param in parameters)
            {
                props.Headers.Add(param.Key, param.Value);
            }
            channel.BasicPublish(ExchangeName, RoutingKey, props, data);
            channel.Close();

        }

        public static IModel GetRabbitChannel(IConnection Connection, string exchangeName, string queueName, string routingKey)
        {
            IModel model = Connection.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("x-expires", 1800000);
            model.QueueDeclare(queueName, false, false, true, args);
            model.QueueBind(queueName, exchangeName, routingKey, null);
            return model;
        }
    }
}
