using System;

namespace CQRSlite.Domain
{
    public interface IRepository
    {
        void Save<T>(T aggregate) where T : AggregateRoot;
        T Get<T>(Guid aggregateId, int? expectedVersion = null) where T : AggregateRoot;
        int GetVersion(Guid aggregateId);
    }
}