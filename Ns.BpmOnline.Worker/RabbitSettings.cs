using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{
    public interface IRabbitSettings
    {
        string ExchangeName { get; }
        string QueueName { get; }
        string RoutingKey { get; }
    }

    abstract public class RabbitSettings
    {
        protected string WorkerName { get; }
        public RabbitSettings(string workerName = "")
        {
            WorkerName = String.IsNullOrEmpty(workerName) ? ConfigurationManager.AppSettings["workerName"] : workerName;
        }
    }

    public class ProcessExecutorRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public ProcessExecutorRabbitSettings(string WorkerName = "") : base(WorkerName) { }
        public string ExchangeName => WorkerName;
        public string QueueName => String.Format("{0}_{1}", WorkerName, "PROCESS_EXECUTOR");
        public string RoutingKey => String.Format("{0}_{1}", WorkerName, "PROCESS_EXECUTOR");
    }

    public class ServiceExecutorRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public ServiceExecutorRabbitSettings(string WorkerName = "") : base(WorkerName) { }
        public string ExchangeName => WorkerName;
        public string QueueName => String.Format("{0}_{1}", WorkerName, "SERVICE_EXECUTOR");
        public string RoutingKey => String.Format("{0}_{1}", WorkerName, "SERVICE_EXECUTOR");
    }

    public class UpdateExecutorRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public UpdateExecutorRabbitSettings(string WorkerName = "") : base(WorkerName) { }
        public string ExchangeName => WorkerName;
        public string QueueName => String.Format("{0}_{1}", WorkerName, "UPDATE_EXECUTOR");
        public string RoutingKey => String.Format("{0}_{1}", WorkerName, "UPDATE_EXECUTOR");
        public string AnswerQueueName => String.Format("{0}_{1}", QueueName, "ANSWER");
        public string AnswerRoutingKey => String.Format("{0}_{1}", QueueName, "ANSWER");
    }

    public class UpdateFilesRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public UpdateFilesRabbitSettings(string WorkerName = "") : base(WorkerName) { }
        public string ExchangeName => WorkerName;
        public string QueueName => String.Format("{0}_{1}", WorkerName, "UPDATE_FILES");
        public string RoutingKey => String.Format("{0}_{1}", WorkerName, "UPDATE_FILES");
        public string AnswerQueueName => String.Format("{0}_{1}", QueueName, "ANSWER");
        public string AnswerRoutingKey => String.Format("{0}_{1}", RoutingKey, "ANSWER");
    }
}