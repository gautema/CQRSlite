using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQRSlite.Events
{
    public interface IEventStore
    {
        void Save<T>(IEnumerable<IEvent> events);
        Task SaveAsync<T>(IEnumerable<IEvent> events);
        IEnumerable<IEvent> Get<T>(Guid aggregateId, int fromVersion);
    }
}