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

        public static IModel GetRabbitChannel(IConnection Connection, string exchangeName, string queueName, string routingKey)
        {
            IModel model = Connection.CreateModel();
            model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            model.QueueDeclare(queueName, false, false, false, null);
            model.QueueBind(queueName, exchangeName, routingKey, null);
            return model;
        }
    }
}
