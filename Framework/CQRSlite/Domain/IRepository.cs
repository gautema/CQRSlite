using System;

namespace CQRSlite.Domain
{
    public interface IRepository
    {
        void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot;
        T Get<T>(Guid aggregateId) where T : AggregateRoot;
    }
}