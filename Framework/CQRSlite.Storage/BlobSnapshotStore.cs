extern alias AzureStorageNet;
using CQRSlite.Snapshotting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Storage.Net;
using Storage.Net.Blobs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureStorageNet;


namespace CQRSlite.Storage {
    public class BlobSnapshotStore : ISnapshotStore {

        public static string CONNECTIONSTRING_KEY = "CQRSlite.Storage.BlobSnapshotStore.ConnectionString";

        IBlobStorage _storage;
        IBlobSnapshotStorePathProvider _path;
         
        public BlobSnapshotStore(IConfiguration config, IBlobSnapshotStorePathProvider path) {

            if (config[CONNECTIONSTRING_KEY].StartsWith("azure")) {
                AzureStorageNet.Storage.Net.Factory.UseAzureStorage(StorageFactory.Modules);
            }
            _storage = StorageFactory.Blobs.FromConnectionString(config[CONNECTIONSTRING_KEY]);
            
            _path = path;

        }

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings() {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        public async Task<Snapshot> Get(Guid id, CancellationToken cancellationToken = default) {
            var json = await _storage.ReadTextAsync(_path.GetSnapshotPath(id));

            if(!string.IsNullOrEmpty(json)) {
                return (Snapshot)JsonConvert.DeserializeObject(json, JsonSettings);
            }
            return null;

        }

        public async Task Save(Snapshot snapshot, CancellationToken cancellationToken = default) {
            var json = JsonConvert.SerializeObject(snapshot, JsonSettings);
            await _storage.WriteTextAsync(_path.GetSnapshotPath(snapshot.Id), json, null, cancellationToken);
            Blob b = await _storage.GetBlobAsync(_path.GetSnapshotPath(snapshot.Id), cancellationToken);
            b.Properties["Version"] = snapshot.Version.ToString() ;
            b.Properties["Id"] = snapshot.Id.ToString();
            await _storage.SetBlobAsync(b, cancellationToken);


        }
    }
}
