using MetaQuotes.MT5CommonAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IMTDeal;

namespace TradingWatchdog.Logic.Extensions
{
    public static class CIMTDealExtensions
    {
        public static string FormatDeal(this CIMTDeal deal, decimal balance)
        {
            if (deal == null)
                throw new ArgumentNullException(nameof(deal));

            return $"Deal #{deal.Deal()}, User '{deal.Login()}', Balance '{balance}', {FormatDealAction(deal)} {deal.Symbol()} {deal.Volume() / (double)10000} " +
                $"lots at {DateTimeOffset.FromUnixTimeMilliseconds(deal.TimeMsc()).ToString("dd.MM.yyyy HH:mm:ss.fff")}";
        }

        public static string FormatDealAction(this CIMTDeal deal)
        {
            switch (deal.Action())
            {
                case 0:
                    return "Buy";
                case 1:
                    return "Sell";
                default:
                    return "Unknown";
            }
        }
    }
}
