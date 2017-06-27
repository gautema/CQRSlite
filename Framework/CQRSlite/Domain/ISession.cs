using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    public interface ISession
    {
        Task Add<T>(T aggregate, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot;
        Task<T> Get<T>(Guid id, int? expectedVersion = null, CancellationToken cancellationToken = default(CancellationToken)) where T : AggregateRoot;
        Task Commit(CancellationToken cancellationToken = default(CancellationToken));
    }
}