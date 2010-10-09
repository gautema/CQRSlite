using System;
using CQRSlite.Domain;

namespace CQRSlite.Eventing
{
    public interface ISnapshotStore<T>  where T : AggregateRoot
    {
        Snapshot<T> Get(Guid id);
        void Save(T aggregateRoot);
    }
}
