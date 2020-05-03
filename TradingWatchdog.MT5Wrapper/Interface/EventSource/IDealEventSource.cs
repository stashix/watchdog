using MetaQuotes.MT5CommonAPI;
using System.Threading.Tasks;

namespace MT5Wrapper.Interface.EventSource
{
	public delegate Task DealEventHandler(object control, CIMTDeal deal);

	public interface IDealEventSource
	{
		event DealEventHandler DealAddEventHandler;
	}
}
