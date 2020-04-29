using MT5Wrapper;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using TradingWatchdog.Logic.Models;
using Timer = System.Timers.Timer;

namespace TradingWatchdog.Logic.Services
{
    public class Watchdog : IWatchdog
    {
        private readonly ILogger _logger;
        private readonly IDealChecker _dealChecker;
        private readonly Configuration _configuration;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Timer _clearTimer;

        private bool disposed = false;
        private List<IServerObserver> _dealSources;
        private ConcurrentBag<Deal> _deals;

        public Watchdog(ILogger logger, IDealChecker dealChecker, Configuration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dealChecker = dealChecker ?? throw new ArgumentNullException(nameof(dealChecker));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cancellationTokenSource = new CancellationTokenSource();
            _clearTimer = new Timer();

            _dealSources = new List<IServerObserver>();
            _deals = new ConcurrentBag<Deal>();
        }

        public void Start()
        {
            try
            {
                foreach (ConnectionParams connectionParam in _configuration.ServerConnections)
                {
                    IServerObserver serverObserver = new ServerObserver(_logger, _dealChecker, _deals);
                    _dealSources.Add(serverObserver);
                    serverObserver.Connect(connectionParam);
                }

                _clearTimer.Elapsed += new ElapsedEventHandler(OnClearExpiredDeals);
                _clearTimer.Interval = _configuration.TimescaleMs * _configuration.ClearTresholdMultiplier;
                _clearTimer.AutoReset = true;

                _clearTimer.Start();
            }
            catch (Exception)
            {
                Stop();
                throw;
            }
        }

        public void Stop()
        {
            _clearTimer.Stop();

            foreach (IServerObserver serverObserver in _dealSources)
            {
                serverObserver.Disconnect();
                serverObserver.Dispose();
            }

            _dealSources.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                foreach (IServerObserver serverObserver in _dealSources)
                {
                    serverObserver.Disconnect();
                    serverObserver.Dispose();
                }
            }

            disposed = true;
        }

        private void OnClearExpiredDeals(object source, ElapsedEventArgs e)
        {
            DateTimeOffset currentTimestamp = DateTimeOffset.UtcNow;
            uint treshold = _configuration.TimescaleMs * _configuration.ClearTresholdMultiplier;

            _logger.Information($"{Thread.CurrentThread.ManagedThreadId} Pre-removal collection count {_deals.Count}");

            foreach (Deal deal in _deals)
            {
                DateTimeOffset dealTimestamp = TimeZoneInfo.ConvertTimeToUtc(DateTimeOffset.FromUnixTimeMilliseconds(deal.Timestamp).DateTime);

                if ((currentTimestamp - dealTimestamp).TotalMilliseconds > treshold && _deals.TryTake(out Deal dealToRemove))
                    _logger.Information($"{Thread.CurrentThread.ManagedThreadId} Removed deal {dealToRemove} from collection.");
            }

            _logger.Information($"{Thread.CurrentThread.ManagedThreadId} Post-removal collection count {_deals.Count}");          
        }
    }
}
