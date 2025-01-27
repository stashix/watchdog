﻿using System;
using System.Collections.Generic;
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

            if (volumeToBalanceRatio == 0 || volumeToBalanceRatio > 100)
                throw new ArgumentException($"{nameof(volumeToBalanceRatio)} must be between 1 and 100.");

            _timescaleMs = timescaleMs;
            _volumeToBalanceRatio = volumeToBalanceRatio;

        }

        public List<DealWarning> CheckDeal(IMT5Api mt5Api, Deal deal, IEnumerable<Deal> previousDeals)
        {
            if (mt5Api == null)
                throw new ArgumentNullException(nameof(mt5Api));

            if (deal == null)
                throw new ArgumentNullException(nameof(deal));


            List<DealWarning> warnings = new List<DealWarning>();

            if (previousDeals == null)
                return warnings;

            foreach (Deal previousDeal in previousDeals)
            {
                if (deal.Action != previousDeal.Action)
                    continue;

                if (deal.DealId == previousDeal.DealId && deal.ServerName == previousDeal.ServerName)
                    continue;

                long timescaleChange = deal.Timestamp - previousDeal.Timestamp;
                bool timescaleSuspect = timescaleChange <= _timescaleMs;
                if (!timescaleSuspect)
                    continue;

                if (deal.Symbol != previousDeal.Symbol)
                    continue;

                decimal dealBalance = mt5Api.GetUserBalance(deal.UserId);
                decimal previousDealBalance = mt5Api.GetUserBalance(previousDeal.UserId);

                var dealRatio = deal.Volume / dealBalance;
                var previousDealRatio = previousDeal.Volume / previousDealBalance;

                var change = Math.Abs((dealRatio - previousDealRatio) / previousDealRatio * 100);
                bool volumeToBalanceRatioSuspect = change <= _volumeToBalanceRatio;
                if (!volumeToBalanceRatioSuspect)
                    continue;

                DealWarning warning = new DealWarning()
                {
                    Deal = deal,
                    SuspectDeal = previousDeal,
                    Reasons = new List<string>()
                    {
                        $"Occured {timescaleChange} ms after, treshold is {_timescaleMs}",
                        $"Currency on both is {deal.Symbol}",
                        $"VolumeToBalance ratio change is {change}, treshold is {_volumeToBalanceRatio}"
                    }
                };

                warnings.Add(warning);
            }

            return warnings;
        }
    }
}
