using CQRSlite.Events;

namespace CQRSlite.Messages
{
	public interface IEventHandler<T> : IHandler<T> where T : Event
	{
	}
}