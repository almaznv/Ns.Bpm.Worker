using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Ns.BpmOnline.Worker
{
    class BpmServersConfigSection : ConfigurationSection
    {
        [ConfigurationProperty( "Servers" )]
        public ServersCollection ServerItems
        {
        get { return ( (ServersCollection)( base["Servers"] ) ); }
        }
    }

    [ConfigurationCollection(typeof(ServerElement))]
    public class ServersCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement)(element)).Name;
        }

        public ServerElement this[int idx]

        {

            get { return (ServerElement)BaseGet(idx); }

        }

    }

    public class ServerElement : ConfigurationElement
    {

        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)(base["name"])); }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("host", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Host
        {
            get { return ((string)(base["host"])); }
            set { base["host"] = value; }
        }

        [ConfigurationProperty("login", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Login
        {
            get { return ((string)(base["login"])); }
            set { base["login"] = value; }
        }

        [ConfigurationProperty("pass", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Password
        {
            get { return ((string)(base["pass"])); }
            set { base["pass"] = value; }
        }

    }
}
