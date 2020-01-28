extern alias AzureStorageNet;
using CQRSlite.Events;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Storage.Net;
using Storage.Net.Blobs;
using Newtonsoft.Json;
using AzureStorageNet;

namespace CQRSlite.Storage {
    public class BlobEventStore : IEventStore {

        public static string CONNECTIONSTRING_KEY = "CQRSlite.Storage.BlobEventStore.ConnectionString";

        IBlobStorage _storage;
        IBlobEventStorePathProvider _path;
        IEventPublisher _publisher;
        public BlobEventStore(IConfiguration config, IBlobEventStorePathProvider path, IEventPublisher publisher) {
            
            if(config[CONNECTIONSTRING_KEY].StartsWith("azure")) {
                AzureStorageNet.Storage.Net.Factory.UseAzureStorage(StorageFactory.Modules);
            }
            _storage = StorageFactory.Blobs.FromConnectionString(config[CONNECTIONSTRING_KEY]);
            _path = path;
            _publisher = publisher;

        }
        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default) {

            IEvent lastEvent = null;
            if (fromVersion <= -1) {
                fromVersion = 0;
            }
            fromVersion++;
            IList<IEvent> events = new List<IEvent>();
            int lastVersion = fromVersion;
            do {
                var json = await _storage.ReadTextAsync(_path.GetEventPath(aggregateId, lastVersion), null, cancellationToken);
                if (!string.IsNullOrEmpty(json)) {
                    lastEvent = (IEvent)JsonConvert.DeserializeObject(json, JsonSettings);
                    events.Add(lastEvent);
                } else {
                    lastEvent = null;
                }
                lastVersion++;
            } while (lastEvent != null);


            return events;
        }

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings() {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default) {

            List<Task> tasksStore = new List<Task>();
            List<Task> tasksAttributes = new List<Task>();
            List<Task> tasksPublishers = new List<Task>();
            foreach (IEvent e in events) {
                var json = JsonConvert.SerializeObject(e, JsonSettings);
                await _storage.WriteTextAsync(_path.GetEventPath(e.Id, e.Version), json, null, cancellationToken);

                Blob blob = await _storage.GetBlobAsync(_path.GetEventPath(e.Id, e.Version), cancellationToken);
                blob.Properties["Version"] = e.Version.ToString();
                blob.Properties["Id"] = e.Id.ToString();
                blob.Properties["Type"] = e.GetType().ToString();
                await _storage.SetBlobAsync(blob, cancellationToken);

                await _publisher.Publish<IEvent>(e, cancellationToken);
            }

        }
    }   
}
