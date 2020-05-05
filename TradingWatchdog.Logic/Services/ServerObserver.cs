using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MetaQuotes.MT5CommonAPI;
using MT5Wrapper;
using Serilog;
using TradingWatchdog.Logic.Extensions;
using TradingWatchdog.Logic.Models;

namespace TradingWatchdog.Logic.Services
{
    public class ServerObserver : IServerObserver
    {
        private readonly ILogger _logger;
        private readonly IDealChecker _dealChecker;
        private readonly ConcurrentBag<Deal> _deals;

        private readonly MT5Api _mT5ApiDeals;
        private readonly MT5Api _mT5ApiRequests;

        private bool isRunning = false;
        private bool disposed = false;

        public ServerObserver(ILogger logger, IDealChecker dealChecker, ConcurrentBag<Deal> deals)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dealChecker = dealChecker ?? throw new ArgumentNullException(nameof(dealChecker));
            _deals = deals ?? throw new ArgumentNullException(nameof(deals));
            _mT5ApiRequests = new MT5Api();
            _mT5ApiDeals = new MT5Api();
        }

        public void Connect(ConnectionParams connectionParams)
        {
            if (connectionParams == null)
                throw new ArgumentNullException(nameof(connectionParams));

            if (isRunning)
                return;

            try
            {
                _mT5ApiRequests.Connect(connectionParams);

                _mT5ApiDeals.DealEvents.DealAddEventHandler += DealAdded;
                _mT5ApiDeals.Connect(connectionParams);

                isRunning = true;
            }
            catch (Exception ex)
            {
                isRunning = false;
                _logger.Error(ex, $"Failed to connect to Server '{connectionParams.Name}', IP '{connectionParams.IP}'. Reason: '{ex.Message}'.");
                throw;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (!isRunning)
                    return;

                _mT5ApiDeals.DealEvents.DealAddEventHandler -= DealAdded;

                _mT5ApiDeals.Disconnect();
                _mT5ApiRequests.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to disconnect from Server. Reason: '{ex.Message}'.");
                throw;
            }
            finally
            {
                isRunning = false;
            }
        }

        public void DealAdded(object control, CIMTDeal cimtDeal)
        {
            Deal deal = new Deal(cimtDeal, _mT5ApiRequests.Name);
            _deals.Add(deal);

            decimal balance = _mT5ApiRequests.GetUserBalance(cimtDeal.Login());
            _logger.Information(cimtDeal.FormatDeal(balance, _mT5ApiRequests.Name));

            RunChecks(deal, _deals.Where(x => x.Action == deal.Action && x.Timestamp <= deal.Timestamp).ToList());
        }

        private void RunChecks(Deal deal, IEnumerable<Deal> previousDeals)
        {
            List<DealWarning> dealWarnings = _dealChecker.CheckDeal(_mT5ApiRequests, deal, previousDeals);

            foreach (DealWarning dealWarning in dealWarnings)
                _logger.Warning($"{dealWarning}");
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
                if (_mT5ApiDeals != null)
                {
                    _mT5ApiDeals.DealEvents.DealAddEventHandler -= DealAdded;

                    _mT5ApiDeals.Disconnect();
                    _mT5ApiDeals.Dispose();
                }

                if (_mT5ApiRequests != null)
                {
                    _mT5ApiRequests.Disconnect();
                    //_mT5ApiRequests.Dispose();
                }
            }

            disposed = true;
        }
    }
}
