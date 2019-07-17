using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    /// <summary>
    /// Defines a repository where aggregates should be saved to and fetched from.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Saves aggregate
        /// </summary>
        /// <typeparam name="T">Type of aggregate</typeparam>
        /// <param name="aggregate">Aggregate object to be saved</param>
        /// <param name="expectedVersion">Expected version saved from earlier. -1 if new.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task of operation</returns>
        Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot;

        /// <summary>
        /// Fetches aggregate
        /// </summary>
        /// <typeparam name="T">Type of aggregate</typeparam>
        /// <param name="aggregateId">Id of aggregate</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>Task with aggregate as result</returns>
        Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot;
    }
}