using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Contracts.Bus.Handlers
{
	public interface Handles<T> where T: Message
    {
        void Handle(T message);
    }
}