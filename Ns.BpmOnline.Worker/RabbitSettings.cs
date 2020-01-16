using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected readonly string targetServerName;

        public RabbitSettings(string TargetServerName = "")
        {
            targetServerName = String.IsNullOrEmpty(TargetServerName) ? Config.GetBpmServer("TargetHost").Name : TargetServerName;
        }
    }

    public class ProcessExecutorRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public ProcessExecutorRabbitSettings(string TargetServerName = "") : base(TargetServerName) { }
        public string ExchangeName => targetServerName;
        public string QueueName => String.Format("{0}_{1}", targetServerName, "PROCESS_EXECUTOR");
        public string RoutingKey => String.Format("{0}_{1}", targetServerName, "PROCESS_EXECUTOR");
    }

    public class ServiceExecutorRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public ServiceExecutorRabbitSettings(string TargetServerName = "") : base(TargetServerName) { }
        public string ExchangeName => targetServerName;
        public string QueueName => String.Format("{0}_{1}", targetServerName, "SERVICE_EXECUTOR");
        public string RoutingKey => String.Format("{0}_{1}", targetServerName, "SERVICE_EXECUTOR");
    }

    public class UpdateExecutorRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public UpdateExecutorRabbitSettings(string TargetServerName = "") : base(TargetServerName) { }
        public string ExchangeName => targetServerName;
        public string QueueName => String.Format("{0}_{1}", targetServerName, "UPDATE_EXECUTOR");
        public string RoutingKey => String.Format("{0}_{1}", targetServerName, "UPDATE_EXECUTOR");
        public string AnswerQueueName => String.Format("{0}_{1}", QueueName, "ANSWER");
        public string AnswerRoutingKey => String.Format("{0}_{1}", QueueName, "ANSWER");
    }

    public class UpdateFilesRabbitSettings : RabbitSettings, IRabbitSettings
    {
        public UpdateFilesRabbitSettings(string TargetServerName = "") : base(TargetServerName) { }
        public string ExchangeName => targetServerName;
        public string QueueName => String.Format("{0}_{1}", targetServerName, "UPDATE_FILES");
        public string RoutingKey => String.Format("{0}_{1}", targetServerName, "UPDATE_FILES");
        public string AnswerQueueName => String.Format("{0}_{1}", QueueName, "ANSWER");
        public string AnswerRoutingKey => String.Format("{0}_{1}", RoutingKey, "ANSWER");
    }
}