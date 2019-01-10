using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{
    public static class Config
    {

        static public ServerElement GetBpmServer(string type)
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            BpmServersConfigSection section = (BpmServersConfigSection)cfg.Sections["BpmServers"];

            return section.ServerItems.Cast<ServerElement>().FirstOrDefault(x => x.Type == type);
        }
    }
}
