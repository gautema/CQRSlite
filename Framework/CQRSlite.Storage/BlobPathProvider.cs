using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage {
    public class BlobPathProvider : IBlobEventStorePathProvider, IBlobSnapshotStorePathProvider {

        private string _eventPrefix = null;
        private string _snapshotPrefix = null;
        public BlobPathProvider(string eventPrefix, string snapshotPrefix) {
            _eventPrefix = eventPrefix;
            _snapshotPrefix = snapshotPrefix;
        }
        
        string IBlobEventStorePathProvider.GetAggregatePath(Guid aggregateId) {
            return global::Storage.Net.StoragePath.Combine(_eventPrefix, 
                aggregateId.ToString());
        }

        string IBlobEventStorePathProvider.GetEventPath(Guid id, int version) {
            return global::Storage.Net.StoragePath.Combine(_eventPrefix,
                id.ToString(),
                version.ToString().PadLeft(int.MaxValue.ToString().Length, '0') + ".json");

        }

        string IBlobSnapshotStorePathProvider.GetSnapshotPath(Guid aggregateId) {
            return global::Storage.Net.StoragePath.Combine(_snapshotPrefix, aggregateId.ToString() + ".json");
        }
    }
}
