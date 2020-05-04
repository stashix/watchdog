using MT5Wrapper;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TradingWatchdog.Logic.Configuration;
using TradingWatchdog.Logic.Models;
using TradingWatchdog.Logic.Services;

namespace TradingWatchdog.App
{
    class Program
    {
        static void Main(string[] args)
        {
            //decimal dealBalance = 749716.53m;
            //decimal previousDealBalance = 749716.71m;

            //var dealRatio = 2100 / dealBalance;
            //var previousDealRatio = 300 / previousDealBalance;

            //var change = Math.Abs((dealRatio - previousDealRatio) / previousDealRatio * 100);

            AppConfiguration configuration = AppConfiguration.Instance;
            IDealChecker dealChecker = new DealChecker(configuration.TimescaleMs, configuration.VolumeToBalanceRatio);

            using (Logger logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger())
            using (IWatchdog watchdog = new Watchdog(logger, dealChecker, configuration))
            {
                watchdog.Start();

                Console.ReadLine();

                watchdog.Stop();
            }
        }
    }
}
