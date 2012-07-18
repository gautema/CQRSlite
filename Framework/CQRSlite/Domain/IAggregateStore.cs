using System;

namespace CQRSlite.Domain
{
    public interface IAggregateStore
    {
        void Save<T>(T aggregate) where T : AggregateRoot;
        T Get<T>(Guid aggregateId) where T : AggregateRoot;
        int GetVersion(Guid aggregateId);
    }
}