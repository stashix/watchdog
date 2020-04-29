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
    public class DealWarning
    {
        public DealWarning()
        {
            SuspiciousDeals = new List<Deal>();
        }

        [DataMember]
        public Deal Deal { get; set; }

        [DataMember]
        public List<Deal> SuspiciousDeals { get; set; }

        public override string ToString()
        {
            return $"Deal '{Deal}' is suspect in relation to deals '[{string.Join(", ", SuspiciousDeals)}]'.";
        }
    }
}
