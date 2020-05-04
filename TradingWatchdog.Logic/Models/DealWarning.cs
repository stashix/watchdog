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
            Reasons = new List<string>();
        }

        [DataMember]
        public Deal Deal { get; set; }

        [DataMember]
        public Deal SuspectDeal { get; set; }

        [DataMember]
        public List<string> Reasons { get; set; }

        public override string ToString()
        {
            return $"Deal '{Deal}' is suspect in relation to Deal '{SuspectDeal}'. Reasons: [{string.Join(", ", Reasons)}]";
        }
    }
}
