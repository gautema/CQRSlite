using CQRSlite.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Bus
{
    public interface IHandlerRegistrar
    {
        void RegisterHandler<T, H>(Func<T, CancellationToken,Task> handler) where T : class, IMessage where H : class, IMessageHandler<T>;
    }
}
