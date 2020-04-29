using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TradingWatchdog.Logic.Models
{
    [Serializable]
    [DataContract]
    public enum DealAction
    {
        [EnumMember]
        Buy = 0,

        [EnumMember]
        Sell = 1,

        [EnumMember]
        Unknown = 100
    }
}
