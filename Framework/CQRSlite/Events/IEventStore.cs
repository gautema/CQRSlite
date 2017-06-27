using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Events
{
    public interface IEventStore
    {
        Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken));
        Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken));
    }
}