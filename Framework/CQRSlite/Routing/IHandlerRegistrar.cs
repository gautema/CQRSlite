using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Messages;

namespace CQRSlite.Routing
{
    /// <summary>
    /// Interface for handle registation.
    /// </summary>
    public interface IHandlerRegistrar
    {
        /// <summary>
        /// Register the a handler for a given message.
        /// </summary>
        /// <typeparam name="T">Message type to register a handler for</typeparam>
        /// <param name="handler">Function to handle message type</param>
        void RegisterHandler<T>(Func<T, CancellationToken,Task> handler) where T : class, IMessage;
    }
}
