using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage {
    public interface IBlobEventStorePathProvider {
        string GetEventPath(Guid id, int version);
        string GetAggregatePath(Guid aggregateId);
    }
}
