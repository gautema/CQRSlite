using System;

namespace CQRSlite.Domain
{
    public interface IRepository<T> where T : AggregateRoot
    {
        void Add(T aggregate);
        T Get(Guid id, int? expectedVersion = null);
    }
}