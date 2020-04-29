using MetaQuotes.MT5CommonAPI;
using MT5Wrapper;
using MT5Wrapper.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingWatchdog.Logic.Models;

namespace TradingWatchdog.Logic.Services
{
    public interface IDealChecker
    {
        Task<DealWarning> CheckDeal(IMT5Api mt5Api, Deal deal, IEnumerable<Deal> previousDeals);
    }
}
