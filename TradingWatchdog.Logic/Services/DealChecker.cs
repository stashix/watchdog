using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MetaQuotes.MT5CommonAPI;
using MT5Wrapper.Interface;
using TradingWatchdog.Logic.Models;

namespace TradingWatchdog.Logic.Services
{
    public class DealChecker : IDealChecker
    {
        private readonly uint _timescaleMs;
        private readonly uint _volumeToBalanceRatio;

        public DealChecker(uint timescaleMs, uint volumeToBalanceRatio)
        {
            if (timescaleMs == 0)
                throw new ArgumentException($"{nameof(timescaleMs)} must be greater than zero.");

            if(volumeToBalanceRatio == 0 || volumeToBalanceRatio > 100)
                throw new ArgumentException($"{nameof(volumeToBalanceRatio)} must be between 1 and 100.");

            _timescaleMs = timescaleMs;
            _volumeToBalanceRatio = volumeToBalanceRatio;

        }

        public async Task<DealWarning> CheckDeal(IMT5Api mt5Api, Deal deal, IEnumerable<Deal> previousDeals)
        {
            if (mt5Api == null)
                throw new ArgumentNullException(nameof(mt5Api));

            if (deal == null)
                throw new ArgumentNullException(nameof(deal));

            return await Task.Run(() =>
            {
                List<Deal> suspiciousDeals = new List<Deal>();

                if (previousDeals == null)
                    return null;

                foreach (Deal previousDeal in previousDeals)
                {
                    if (deal.DealId == previousDeal.DealId && deal.ServerName == previousDeal.ServerName)
                        continue;

                    bool timescaleSuspect = deal.Timestamp - previousDeal.Timestamp <= _timescaleMs;
                    if (!timescaleSuspect)
                        continue;

                    bool currencySuspect = deal.Symbol == previousDeal.Symbol;
                    if (!currencySuspect)
                        continue;

                    decimal dealBalance = mt5Api.GetUserBalance(deal.UserId);
                    decimal previousDealBalance = mt5Api.GetUserBalance(previousDeal.UserId);

                    var dealRatio = deal.Volume / dealBalance;
                    var previousDealRatio = previousDeal.Volume / previousDealBalance;

                    var change = Math.Abs((dealRatio - previousDealRatio) / previousDealRatio * 100);
                    bool volumeToBalanceRatioSuspect = change <= _volumeToBalanceRatio;
                    if (!volumeToBalanceRatioSuspect)
                        continue;

                    suspiciousDeals.Add(previousDeal);
                }

                if (suspiciousDeals.Count > 0)
                {
                    DealWarning warning = new DealWarning()
                    {
                        Deal = deal,
                        SuspiciousDeals = suspiciousDeals
                    };

                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {warning}");

                    return warning;
                }

                return null;
            });
        }
    }
}
