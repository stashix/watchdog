using MT5Wrapper;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TradingWatchdog.Logic.Models;
using TradingWatchdog.Logic.Services;

namespace TradingWatchdog.App
{
    class Program
    {
        //static MT5Api mt5Api;

        static void Main(string[] args)
        {
            decimal dealBalance = 749716.53m;
            decimal previousDealBalance = 749716.71m;

            var dealRatio = 2100 / dealBalance;
            var previousDealRatio = 300 / previousDealBalance;

            var change = Math.Abs((dealRatio - previousDealRatio) / previousDealRatio * 100);

            //using(Logger logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day).CreateLogger())
            //using (mt5Api = new MT5Api())
            //{
            //    ConcurrentBag<Deal> deals = new ConcurrentBag<Deal>();
            //    Configuration configuration = Configuration.Instance;
            //    IDealChecker dealChecker = new DealChecker(configuration.TimescaleMs, configuration.VolumeToBalanceRatio);
            //    IDealListener dealListener = new DealListener(logger, mt5Api, dealChecker, deals);

            //    List<ConnectionParams> connectionParams = new List<ConnectionParams>()
            //    {
            //        new ConnectionParams()
            //        {
            //            IP = "103.40.209.22:443",
            //            Login = 1005,
            //            Password = "qg3ceury",
            //            Name = "TestConnection"
            //        }
            //    };

            //    mt5Api.DealEvents.DealAddEventHandler += dealListener.DealAdded;
            //    mt5Api.Connect(connectionParams[0]);
            //    mt5Api.Connect(connectionParams[0]);

            //    Console.ReadLine();

            //    mt5Api.Disconnect();
            //    mt5Api.DealEvents.DealAddEventHandler -= dealListener.DealAdded;
            //}

            using (var mt5Api = new MT5Api())
            {
                List<ConnectionParams> connectionParams = new List<ConnectionParams>()
                {
                    new ConnectionParams()
                    {
                        IP = "103.40.209.22:443",
                        Login = 1005,
                        Password = "qg3ceury",
                        Name = "TestConnection"
                    }
                };

                mt5Api.Connect(connectionParams[0]);

                var balance = mt5Api.GetUserBalance(4002);
            }

            Configuration configuration = Configuration.Instance;
            IDealChecker dealChecker = new DealChecker(configuration.TimescaleMs, configuration.VolumeToBalanceRatio);

            using (Logger logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day).CreateLogger())
            using (IWatchdog watchdog = new Watchdog(logger, dealChecker, configuration))
            {
                logger.Information($"{Thread.CurrentThread.ManagedThreadId}");

                watchdog.Start();

                Console.ReadLine();

                watchdog.Stop();
            }
        }
    }
}
