using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaQuotes.MT5CommonAPI;
using MT5Wrapper.Interface;
using Serilog;
using TradingWatchdog.Logic.Extensions;
using TradingWatchdog.Logic.Models;

namespace TradingWatchdog.Logic.Services
{
    public class DealListener : IDealListener
    {
        private readonly IDealChecker _dealChecker;
        private readonly IMT5Api _mtp5Api;
        private readonly ILogger _logger;
        private readonly ConcurrentBag<Deal> _deals;

        public DealListener(ILogger logger, IMT5Api mt5Api, IDealChecker dealChecker, ConcurrentBag<Deal> deals)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mtp5Api = mt5Api ?? throw new ArgumentNullException(nameof(mt5Api));
            _dealChecker = dealChecker ?? throw new ArgumentNullException(nameof(dealChecker));
            _deals = deals ?? throw new ArgumentNullException(nameof(deals));
        }

        public async Task DealAdded(object control, CIMTDeal cimtDeal)
        {
            try
            {
                ulong login = cimtDeal.Login();                
                //decimal balance = _mtp5Api.GetUserBalance(login);
                _logger.Information($"{System.Threading.Thread.CurrentThread.ManagedThreadId} " + cimtDeal.FormatDeal(0));

                Deal deal = new Deal(cimtDeal, _mtp5Api.Name);
                _deals.Add(deal);

                await RunCheck(deal, _deals);
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task RunCheck(Deal deal, IEnumerable<Deal> previousDeals)
        {
            var dealWarning = await _dealChecker.CheckDeal(_mtp5Api, deal, _deals.ToList());

            if (dealWarning != null)
                _logger.Warning($"{dealWarning}");
        }
    }
}
