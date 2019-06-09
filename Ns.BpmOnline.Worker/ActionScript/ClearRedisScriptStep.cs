using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ns.BpmOnline.Worker.Parameters;

namespace Ns.BpmOnline.Worker.ActionScript
{

    public class ClearRedisScriptStep : RunCmdScriptStep, IActionScriptStep
    {
        private static readonly string redisPath = @System.Configuration.ConfigurationManager.AppSettings["redisPath"];

        //ClearRedis
        public ClearRedisScriptStep(ServerElement Server, string redisHost, string redisdb) : base(Server)
        {

            var redisCliSettings = new RedisCliSettings()
            {
                RedisPath = @redisPath,
                RedisHost = (String.IsNullOrEmpty(redisHost)) ? "localhost" : redisHost,
                RedisDB = (String.IsNullOrEmpty(redisdb) || redisdb == "0") ? "flushall" : "-n "+ redisdb + " FLUSHDB"
            };
            
            SetWorkingDirectory(Path.GetDirectoryName(redisPath));
            SetCmdCommand(redisCliSettings.GetCmdParametersStr());
        }

        public override string GetName()
        {
            return "ClearRedis";
        }
    }
}
