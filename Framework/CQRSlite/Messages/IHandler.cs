using System.Threading.Tasks;

namespace CQRSlite.Messages
{
    public interface IHandler<in T> : IMessageHandler<T> where T : IMessage
    {
        Task Handle(T message);
    }
}