using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Queries
{
    /// <summary>
    /// Defines a query sender.
    /// </summary>
    public interface IQueryProcessor
    {
        /// <summary>
        /// Send a query to a single query handler function.
        /// </summary>
        /// <typeparam name="TResponse">Return type</typeparam>
        /// <param name="query">Query object to be sent</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task representing sending</returns>
        Task<TResponse> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
    }
}