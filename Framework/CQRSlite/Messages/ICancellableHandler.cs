using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Messages
{
    public interface ICancellableHandler<in T> : IMessageHandler<T> where T : IMessage
    {
        Task Handle(T message, CancellationToken token = default(CancellationToken));
    }
}