using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Contracts.Bus.Handlers
{
	public interface HandlesEvent<T> : Handles<T> where T : Event
	{
	}
}