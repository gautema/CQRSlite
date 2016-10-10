using System;
using CQRSlite.Domain;

namespace CQRSlite.Cache
{
    public interface ICache
    {
        bool IsTracked(Guid id);
        void Set(Guid id, AggregateRoot aggregate);
        AggregateRoot Get(Guid id);
        void Remove(Guid id);
        void RegisterEvictionCallback(Action<Guid> action);
    }
}
