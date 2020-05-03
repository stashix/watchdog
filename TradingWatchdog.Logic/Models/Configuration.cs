using MT5Wrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradingWatchdog.Logic.Models
{
    public class Configuration
    {
        private static readonly Lazy<Configuration> _instance = new Lazy<Configuration>(() => new Configuration(), LazyThreadSafetyMode.PublicationOnly);

        private Configuration()
        {
            TimescaleMs = uint.TryParse(ConfigurationManager.AppSettings["TimescaleMs"], out uint parsedTimescaleMs)
                ? parsedTimescaleMs : 1000;

            VolumeToBalanceRatio = uint.TryParse(ConfigurationManager.AppSettings["VolumeToBalanceRatio"], out uint parsedVolumeToBalanceRatio)
                ? parsedVolumeToBalanceRatio : 5;

            ClearTresholdMultiplier = uint.TryParse(ConfigurationManager.AppSettings["ClearTresholdMultiplier"], out uint parsedClearTresholdMultiplier)
                ? parsedClearTresholdMultiplier : 5;

            ServerConnections = new List<ConnectionParams>()
            {
                new ConnectionParams()
                {
                    IP = "103.40.209.22:443",
                    Login = 1005,
                    Password = "qg3ceury",
                    Name = "TestConnection"
                }
            };
        }

        public uint TimescaleMs { get; private set; }

        public uint VolumeToBalanceRatio { get; private set; }

        public uint ClearTresholdMultiplier { get; private set; }

        public List<ConnectionParams> ServerConnections { get; private set; }

        public static Configuration Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}
