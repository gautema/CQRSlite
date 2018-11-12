using System.Threading.Tasks;

namespace CQRSlite.Queries
{
    /// <summary>
    /// Defines a handler for a query.
    /// </summary>
    /// <typeparam name="T">Query type being handled</typeparam>
    /// <typeparam name="TResponse">Type being returned by handler</typeparam>
    public interface IQueryHandler<in T, TResponse> where T : IQuery<TResponse>
    {
        /// <summary>
        ///  Handles a query
        /// </summary>
        /// <param name="query">Query being handled</param>
        /// <returns>Task that represents handling of message</returns>
        Task<TResponse> Handle(T query);
    }
}