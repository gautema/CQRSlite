using System.Threading.Tasks;

namespace CQRSlite.Messages
{
    /// <summary>
    /// Defines a handler for a message.
    /// </summary>
    /// <typeparam name="T">Message type being handled</typeparam>
    public interface IHandler<in T> where T : IMessage
    {
        /// <summary>
        ///  Handles a message
        /// </summary>
        /// <param name="message">Message being handled</param>
        /// <returns>Task that represents handling of message</returns>
        Task Handle(T message);
    }
}