using System;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    public interface ISession
    {
        Task Add<T>(T aggregate) where T : AggregateRoot;
        Task<T> Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot;
        Task Commit();
    }
}