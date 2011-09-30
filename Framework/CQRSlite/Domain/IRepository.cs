using System;

namespace CQRSlite.Domain
{
    public interface IRepository<T> where T : AggregateRoot
    {
        void Add(T aggregate);
        void Commit();
        T Get(Guid id);
    }
}