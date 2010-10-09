using System;

namespace CQRSlite.Domain
{
    public interface IRepository<T> where T : AggregateRoot
    {
        void Save(AggregateRoot aggregate, int expectedVersion);
        T GetById(Guid id);
    }
}