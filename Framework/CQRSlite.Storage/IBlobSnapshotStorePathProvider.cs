using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage {
    public interface IBlobSnapshotStorePathProvider {
        string GetSnapshotPath(Guid aggregateId);
    }
}
