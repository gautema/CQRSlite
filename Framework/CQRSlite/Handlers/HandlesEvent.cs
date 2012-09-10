using CQRSlite.Eventing;

namespace CQRSlite.Handlers
{
	public interface HandlesEvent<T> : Handles<T> where T : Event
	{
	}
}