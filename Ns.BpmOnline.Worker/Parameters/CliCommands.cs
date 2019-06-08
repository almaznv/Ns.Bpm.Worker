using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ns.BpmOnline.Worker.Parameters
{
    class RedisCliSettings
    {
        public string RedisPath { get; set; }
        public string RedisHost { get; set; } = "localhost";
        public string RedisDB { get; set; } = "flushall";

        public string GetCmdParametersStr()
        {
            return String.Format("{0} -h {1} {2}", RedisPath, RedisHost, RedisDB);
        }
    }
}
