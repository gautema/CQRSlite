using System;
using System.Threading.Tasks;
using CQRSlite.Domain;

namespace CQRSlite.Caching
{
    /// <summary>
    /// Defines a cache implementation
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Check if aggregate is tracked.
        /// </summary>
        /// <param name="id">Id of aggregate</param>
        /// <returns>Task representing operation. Task result if is tracked</returns>
        Task<bool> IsTracked(Guid id);

        /// <summary>
        /// Adds Aggregate to cache
        /// </summary>
        /// <param name="id">Id of aggregate</param>
        /// <param name="aggregate">The aggregate to cache</param>
        /// <returns>Task representing operation</returns>
        Task Set(Guid id, AggregateRoot aggregate);

        /// <summary>
        /// Get aggregate from cache
        /// </summary>
        /// <param name="id">Id of aggregate</param>
        /// <returns>Task representing operation. Task result is aggregate</returns>
        Task<AggregateRoot> Get(Guid id);

        /// <summary>
        /// Remove aggregate from cache
        /// </summary>
        /// <param name="id">Id of aggregate</param>
        /// <returns>Task representing operation</returns>
        Task Remove(Guid id);

        /// <summary>
        /// Register a callback action to be called when a cached object is evicted from cache.
        /// </summary>
        /// <param name="action">Action to be called</param>
        void RegisterEvictionCallback(Action<Guid> action);
    }
}
