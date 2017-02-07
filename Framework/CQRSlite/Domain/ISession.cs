using System;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    public interface ISession
    {
        void Add<T>(T aggregate) where T : AggregateRoot;
        T Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot;
        Task<T> GetAsync<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot;
        void Commit();
        Task CommitAsync();
    }
}