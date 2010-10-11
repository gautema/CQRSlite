using System;

namespace CQRSlite.Domain
{
    public interface IRepository<T> where T : AggregateRoot
    {
        void Save(T aggregate, int expectedVersion);
        T GetById(Guid id);
    }
}