using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CQRSlite.Domain;

namespace CQRSlite.Events
{
    public interface IEventStore
    {
        Task Save(IEnumerable<IEvent> events);
        Task<IEnumerable<IEvent>> Get(IIdentity aggregateId, int fromVersion);
    }
}