using MetaQuotes.MT5CommonAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingWatchdog.Logic.Models;

namespace TradingWatchdog.Logic.Services
{
    public interface IDealListener
    {
        Task DealAdded(object control, CIMTDeal deal);

        Task RunCheck(Deal deal, IEnumerable<Deal> previousDeals);
    }
}
