using MetaQuotes.MT5CommonAPI;
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
    public class Deal
    {
        public Deal()
        {

        }

        public Deal(CIMTDeal cimtDeal, string serverName)
        {
            if (cimtDeal == null)
                throw new ArgumentNullException(nameof(cimtDeal));

            if (string.IsNullOrWhiteSpace(serverName))
                throw new ArgumentException($"{nameof(serverName)} cannot be null or whitespace.");

            DealId = cimtDeal.Deal();
            UserId = cimtDeal.Login();
            Volume = cimtDeal.Volume();
            Action = cimtDeal.Action() <= 1 ? (DealAction)cimtDeal.Action() : DealAction.Unknown;
            Symbol = cimtDeal.Symbol();
            Timestamp = cimtDeal.TimeMsc();
            ServerName = serverName;
        }

        [DataMember]
        public ulong DealId { get; set; }

        [DataMember]
        public ulong UserId { get; set; }

        [DataMember]
        public ulong Volume { get; set; }

        [DataMember]
        public DealAction Action { get; set; }

        [DataMember]
        public string Symbol { get; set; }

        [DataMember]
        public long Timestamp { get;set; }

        [DataMember]
        public string ServerName { get; set; }

        public override string ToString()
        {
            return $"#{DealId}, Server '{ServerName}', User '{UserId}', {Enum.GetName(typeof(DealAction), Action)} {Symbol} {Volume / (double)10000} " +
                $"lots at {DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).ToString("dd.MM.yyyy HH:mm:ss.fff")}";
        }
    }
}
