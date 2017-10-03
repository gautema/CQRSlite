using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Events
{
    /// <summary>
    /// Defines an event publisher
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publish an event to zero to many handler functions.
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="event">Event object to be sent</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task representing publishing</returns>
        Task Publish<T>(T @event, CancellationToken cancellationToken = default(CancellationToken)) where T : class, IEvent;
    }
}