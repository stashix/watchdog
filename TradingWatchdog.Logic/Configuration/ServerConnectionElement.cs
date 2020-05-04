using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingWatchdog.Logic.Configuration
{
    public class ServerConnectionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("ip", IsRequired = true)]
        public string IP
        {
            get
            {
                return this["ip"] as string;
            }
        }

        [ConfigurationProperty("login", IsRequired = true)]
        public ulong Login
        {
            get
            {
                return (ulong)this["login"];
            }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get
            {
                return this["password"] as string;
            }
        }

    }
}
