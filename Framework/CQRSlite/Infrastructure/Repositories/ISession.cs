using System;
using CQRSlite.Domain;

namespace CQRSlite.Infrastructure.Repositories.Domain
{
    public interface ISession
    {
        void Add<T>(T aggregate) where T : AggregateRoot;
        T Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot;
        void Commit();
    }
}