using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingWatchdog.Logic.Configuration
{
    public class WatchedServersConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("serverConnections")]
        public ServerConnectionCollection ServerConnections
        {
            get
            {
                return this["serverConnections"] as ServerConnectionCollection;
            }
        }

    }
}
