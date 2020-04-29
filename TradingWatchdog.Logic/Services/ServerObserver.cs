using MetaQuotes.MT5CommonAPI;
using MT5Wrapper;
using MT5Wrapper.Interface;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingWatchdog.Logic.Extensions;
using TradingWatchdog.Logic.Models;

namespace TradingWatchdog.Logic.Services
{
    public class ServerObserver : IServerObserver
    {
        private readonly ILogger _logger;
        private readonly IDealChecker _dealChecker;
        private readonly ConcurrentBag<Deal> _deals;

        private MT5Api _mT5ApiDeals;
        private MT5Api _mT5ApiRequests;
        private IDealListener _dealListener;

        private bool disposed = false;

        public ServerObserver(ILogger logger, IDealChecker dealChecker, ConcurrentBag<Deal> deals)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dealChecker = dealChecker ?? throw new ArgumentNullException(nameof(dealChecker));
            _deals = deals ?? throw new ArgumentNullException(nameof(deals));
        }

        public void Connect(ConnectionParams connectionParams)
        {
            if(_mT5ApiDeals != null)
            {
                //if (_dealListener != null)
                //    _mT5ApiDeals.DealEvents.DealAddEventHandler -= DealAdded;

                _mT5ApiDeals.Disconnect();
                _mT5ApiDeals.Dispose();
            }

            if (_mT5ApiRequests != null)
            {
                _mT5ApiRequests.Disconnect();
                _mT5ApiRequests.Dispose();
            }

            _mT5ApiRequests = new MT5Api();
            _mT5ApiRequests.Connect(connectionParams);

            _mT5ApiDeals = new MT5Api();
            //_dealListener = new DealListener(_logger, _mT5ApiDeals, _dealChecker, _deals);
            _mT5ApiDeals.DealEvents.DealAddEventHandler += DealAdded;
            _mT5ApiDeals.Connect(connectionParams);
        }

        public void Disconnect()
        {
            if (_mT5ApiDeals != null)
            {
                //if (_dealListener != null)
                //    _mT5Api.DealEvents.DealAddEventHandler -= DealAdded;

                _mT5ApiDeals.Disconnect();
            }

            if(_mT5ApiRequests != null)
            {
                _mT5ApiRequests.Disconnect();
            }
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
                    _logger.Information($"Disposing {Thread.CurrentThread.ManagedThreadId}");

                    //if (_dealListener != null)
                    //    _mT5Api.DealEvents.DealAddEventHandler -= DealAdded;

                    _mT5ApiDeals.Disconnect();
                    _mT5ApiDeals.Dispose();
                }

                if (_mT5ApiRequests != null)
                {
                    _logger.Information($"Disposing {Thread.CurrentThread.ManagedThreadId}");

                    _mT5ApiRequests.Disconnect();
                    _mT5ApiRequests.Dispose();
                }
            }

            disposed = true;
        }

        public void DealAdded(object control, CIMTDeal cimtDeal)
        {
            try
            {
                Deal deal = new Deal(cimtDeal, _mT5ApiDeals.Name);
                _deals.Add(deal);

                ulong login = cimtDeal.Login();
                decimal balance = _mT5ApiRequests.GetUserBalance(login);
                _logger.Information($"{Thread.CurrentThread.ManagedThreadId} {cimtDeal.FormatDeal(balance)}");

                var dealWarning = _dealChecker.CheckDeal(_mT5ApiRequests, deal, _deals.ToList()).Result;

                if (dealWarning != null)
                    _logger.Warning($"{Thread.CurrentThread.ManagedThreadId} {dealWarning}");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
