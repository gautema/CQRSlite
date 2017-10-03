using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Messages
{
    /// <summary>
    /// Defines a handler for a message with a cancellation token.
    /// </summary>
    /// <typeparam name="T">Message type being handled</typeparam>
    public interface ICancellableHandler<in T> where T : IMessage
    {
        /// <summary>
        ///  Handles a message
        /// </summary>
        /// <param name="message">Message being handled</param>
        /// <param name="token">Cancellation token from sender/publisher.</param>
        /// <returns>Task that represents handling of message</returns>
        Task Handle(T message, CancellationToken token = default(CancellationToken));
    }
}