using MT5Wrapper;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using TradingWatchdog.Logic.Configuration;
using TradingWatchdog.Logic.Models;
using Timer = System.Timers.Timer;

namespace TradingWatchdog.Logic.Services
{
    public class Watchdog : IWatchdog
    {
        private readonly ILogger _logger;
        private readonly IDealChecker _dealChecker;
        private readonly uint _clearTreshold;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Timer _clearTimer;

        private bool isRunning = false;
        private bool disposed = false;
        private List<IServerObserver> _dealSources;
        private ConcurrentBag<Deal> _deals;

        public Watchdog(ILogger logger, IDealChecker dealChecker, uint clearTreshold)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dealChecker = dealChecker ?? throw new ArgumentNullException(nameof(dealChecker));

            if (clearTreshold == 0)
                throw new ArgumentException($"{nameof(clearTreshold)} must be greater than zero.");

            _clearTreshold = clearTreshold;

            _cancellationTokenSource = new CancellationTokenSource();
            _clearTimer = new Timer();

            _dealSources = new List<IServerObserver>();
            _deals = new ConcurrentBag<Deal>();
        }

        public void Start(IEnumerable<ConnectionParams> connectionParams)
        {
            try
            {
                if (isRunning)
                    return;

                foreach (ConnectionParams connectionParam in connectionParams)
                {
                    _logger.Debug($"Starting observer for Server '{connectionParam.Name}', IP: '{connectionParam.IP}'.");

                    IServerObserver serverObserver = new ServerObserver(_logger, _dealChecker, _deals);
                    _dealSources.Add(serverObserver);
                    serverObserver.Connect(connectionParam);

                    _logger.Debug($"Started observer for Server '{connectionParam.Name}', IP: '{connectionParam.IP}'.");
                }

                _clearTimer.Elapsed += new ElapsedEventHandler(OnClearExpiredDeals);
                _clearTimer.Interval = _clearTreshold;
                _clearTimer.AutoReset = true;

                _clearTimer.Start();

                isRunning = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Starting Watchdog failed. Reason '{ex.Message}'.");
                Stop();
                throw;
            }
        }

        public void Stop()
        {
            _clearTimer.Stop();
            _clearTimer.Elapsed -= new ElapsedEventHandler(OnClearExpiredDeals);

            foreach (IServerObserver serverObserver in _dealSources)
            {
                serverObserver.Disconnect();
                serverObserver.Dispose();
            }

            _dealSources.Clear();

            isRunning = false;
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
                Stop();
            }

            disposed = true;
        }

        private void OnClearExpiredDeals(object source, ElapsedEventArgs e)
        {
            DateTimeOffset currentTimestamp = DateTimeOffset.UtcNow;

            _logger.Debug($"Pre-removal collection count {_deals.Count}");

            foreach (Deal deal in _deals)
            {
                DateTimeOffset dealTimestamp = TimeZoneInfo.ConvertTimeToUtc(DateTimeOffset.FromUnixTimeMilliseconds(deal.Timestamp).DateTime);

                if ((currentTimestamp - dealTimestamp).TotalMilliseconds > _clearTreshold && _deals.TryTake(out Deal dealToRemove))
                    _logger.Debug($"Removed deal {dealToRemove} from collection.");
            }

            _logger.Debug($"Post-removal collection count {_deals.Count}");          
        }
    }
}
