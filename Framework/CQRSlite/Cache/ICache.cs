using System;
using CQRSlite.Domain;

namespace CQRSlite.Cache
{
    public interface ICache
    {
        bool IsTracked(IIdentity id);
        void Set(IIdentity id, AggregateRoot aggregate);
        AggregateRoot Get(IIdentity id);
        void Remove(IIdentity id);
        void RegisterEvictionCallback(Action<IIdentity> action);
    }
}
