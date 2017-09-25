using System;
using System.Threading.Tasks;
using CQRSlite.Domain;

namespace CQRSlite.Caching
{
    public interface ICache
    {
        Task<bool> IsTracked(Guid id);
        Task Set(Guid id, AggregateRoot aggregate);
        Task<AggregateRoot> Get(Guid id);
        Task Remove(Guid id);
        void RegisterEvictionCallback(Action<Guid> action);
    }
}
