using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingWatchdog.Logic.Configuration
{
    [ConfigurationCollection(typeof(ServerConnectionElement))]
    public class ServerConnectionCollection : ConfigurationElementCollection
    {
        public ServerConnectionElement this[int index]
        {
            get
            {
                return BaseGet(index) as ServerConnectionElement;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerConnectionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerConnectionElement)element).Name;
        }
    }
}
