using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Queries
{
    /// <summary>
    /// Defines a handler for a query with a cancellation token.
    /// </summary>
    /// <typeparam name="T">Query type being handled</typeparam>
    /// <typeparam name="TResponse">Type being returned by handler</typeparam>
    public interface ICancellableQueryHandler<in T, TResponse> where T : IQuery<TResponse>
    {
        /// <summary>
        ///  Handles a query
        /// </summary>
        /// <param name="message">Query being handled</param>
        /// <param name="token">Cancellation token from sender/publisher.</param>
        /// <returns>Task of return type that represents handling of message</returns>
        Task<TResponse> Handle(T message, CancellationToken token = default);
    }
}