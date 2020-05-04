using MT5Wrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradingWatchdog.Logic.Configuration
{
    public class AppConfiguration
    {
        private static readonly Lazy<AppConfiguration> _instance = new Lazy<AppConfiguration>(() => new AppConfiguration(), LazyThreadSafetyMode.PublicationOnly);

        private AppConfiguration()
        {
            TimescaleMs = uint.TryParse(ConfigurationManager.AppSettings["TimescaleMs"], out uint parsedTimescaleMs)
                ? parsedTimescaleMs : 1000;

            VolumeToBalanceRatio = uint.TryParse(ConfigurationManager.AppSettings["VolumeToBalanceRatio"], out uint parsedVolumeToBalanceRatio)
                ? parsedVolumeToBalanceRatio : 5;

            ClearTresholdMultiplier = uint.TryParse(ConfigurationManager.AppSettings["ClearTresholdMultiplier"], out uint parsedClearTresholdMultiplier)
                ? parsedClearTresholdMultiplier : 3;

            var serverConnections = (WatchedServersConfigSection)ConfigurationManager.GetSection("watchedServers");
            List<ConnectionParams> connectionParams = new List<ConnectionParams>();
            foreach(ServerConnectionElement serverConnection in serverConnections.ServerConnections)
            {
                ConnectionParams connectionParam = new ConnectionParams()
                {
                    Name = serverConnection.Name,
                    IP = serverConnection.IP,
                    Login = serverConnection.Login,
                    Password = serverConnection.Password
                };

                connectionParams.Add(connectionParam);
            }

            ServerConnections = connectionParams;
        }

        public uint TimescaleMs { get; private set; }

        public uint VolumeToBalanceRatio { get; private set; }

        public uint ClearTresholdMultiplier { get; private set; }

        public List<ConnectionParams> ServerConnections { get; private set; }

        public static AppConfiguration Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}
