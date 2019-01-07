using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{
    static class RabbitConnector
    {

        private static readonly string _rabbitMqHost = ConfigurationManager.AppSettings["rabbitMqHost"];
        private static readonly string _rabbitMqLogin = ConfigurationManager.AppSettings["rabbitMqLogin"];
        private static readonly string _rabbitMqPassword = ConfigurationManager.AppSettings["rabbitMqPassword"];

        public static IConnection GetConnection()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri(String.Format("amqp://{0}:{1}@{2}/", _rabbitMqLogin, _rabbitMqPassword, _rabbitMqHost))
            };
            return factory.CreateConnection();
        }
    }
}
