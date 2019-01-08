﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Ns.BpmOnline.Worker
{

    public class BpmProcessExecutor : Executor, IExecutor
    {

        public BpmProcessExecutor(ServerElement server) : base(server) { }

        public void Execute(byte[] data)
        {
            Dictionary<string, string> processParameters = DecodeParameters(data);
            Execute(processParameters);
        }

        public void Execute(Dictionary<string, string> processParameters)
        {
            string processName = GetByKey(processParameters, "ProcessName");

            var bpmConnector = new BpmConnector(server.Host);
            bpmConnector.TryLogin(server.Login, server.Password);
            bpmConnector.RunProcess(processName, processParameters);
        }
    }
}
