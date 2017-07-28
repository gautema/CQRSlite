using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Messages;

namespace CQRSlite.Routing
{
    public interface IHandlerRegistrar
    {
        void RegisterHandler<T>(Func<T, CancellationToken,Task> handler) where T : class, IMessage;
    }
}
