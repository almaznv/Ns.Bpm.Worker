using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{

    public class BpmServiceExecutor : IExecutor
    {
        private ServerElement _server;

        public BpmServiceExecutor(ServerElement server)
        {
            _server = server;
        }

        public void Execute(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);

            var bpmConnector = new BpmConnector(_server.Host);
            bpmConnector.TryLogin(_server.Login, _server.Password);
            bpmConnector.RunService("GET", "NsTestService/Test", new Dictionary<string, string>() { });
        }
    }
}
