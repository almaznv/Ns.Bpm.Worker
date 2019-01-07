using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ns.BpmOnline.Worker
{
    public interface IExecutor
    {
        void Execute(byte[] data);
    }

    public class BpmProcessExecutor : IExecutor
    {
        private ServerElement _server;

        public BpmProcessExecutor(ServerElement server)
        {
            _server = server;
        }

        public void Execute(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);

            var bpmConnector = new BpmConnector(_server.Host);
            bpmConnector.TryLogin(_server.Login, _server.Password);

            Logger.Log("Run process");

            bpmConnector.RunProcess("MqTestProcess",
                new Dictionary<string, string>() {});
        }
    }
}
