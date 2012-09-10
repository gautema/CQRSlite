using CQRSlite.Contracts.Messages;

namespace CQRSlite.Contracts.Handlers
{
	public interface Handles<T> where T: Message
    {
        void Handle(T message);
    }
}