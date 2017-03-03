using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQRSlite.Events
{
    public interface IEventStore
    {
        Task Save<T>(IEnumerable<IEvent> events);
        Task<IEnumerable<IEvent>> Get<T>(Guid aggregateId, int fromVersion);
    }
}