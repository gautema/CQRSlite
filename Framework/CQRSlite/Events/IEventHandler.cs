using CQRSlite.Messages;

namespace CQRSlite.Events
{
	public interface IEventHandler<T> : IHandler<T> where T : IEvent
	{
	}
}