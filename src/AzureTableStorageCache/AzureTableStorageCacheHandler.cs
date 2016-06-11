using AzureTableStorageCache.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Internal;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTableStorageCache
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class AzureTableStorageCacheHandler : IDistributedCache
    {
        private readonly string partitionKey;
        private readonly string accountKey;
        private readonly string accountName;
        private readonly string connectionString;
        private CloudTableClient client;
        private CloudTable azuretable;

        private AzureTableStorageCacheHandler(string tableName, string partitionKey)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw new ArgumentNullException("partitionKey cannot be null or empty");
            }
            this.tableName = tableName;
            this.partitionKey = partitionKey;
            Connect();
        }

        public AzureTableStorageCacheHandler(string accountName, string accountKey, string tableName, string partitionKey)
            : this(tableName, partitionKey)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentNullException("accountName cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(accountKey))
            {
                throw new ArgumentNullException("accountKey cannot be null or empty");
            }

            this.accountName = accountName;
            this.accountKey = accountKey;
        }

        private readonly string tableName;

        public AzureTableStorageCacheHandler(string connectionString, string tableName, string partitionKey)
            : this(tableName, partitionKey)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("Connection string cannot be null or empty");
            }

            this.connectionString = connectionString;
        }

        public void Connect()
        {
            ConnectAsync().Wait();
        }

        public async Task ConnectAsync()
        {
            if (client == null)
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    var creds = new StorageCredentials(accountKey, accountKey);
                    client = new CloudStorageAccount(creds, true).CreateCloudTableClient();
                }
                else
                {
                    client = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
                }
            }
            if (azuretable == null)
            {
                this.azuretable = client.GetTableReference(this.tableName);
                await this.azuretable.CreateIfNotExistsAsync();
            }
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).Result;
        }

        public async Task<byte[]> GetAsync(string key)
        {
            var op = TableOperation.Retrieve(partitionKey, key);
            var result = await azuretable.ExecuteAsync(op);
            return (result?.Result as CachedItem)?.Data;
        }

        public void Refresh(string key)
        {
            this.RefreshAsync(key).Wait();
        }

        public async Task RefreshAsync(string key)
        {
            var op = TableOperation.Retrieve(partitionKey, key);
            var result = await azuretable.ExecuteAsync(op);
            var data = result?.Result as CachedItem;
            if (data != null)
            {
                if (ShouldDelete(data))
                {
                    await RemoveAsync(key);
                    return;
                }
            }
        }

        private bool ShouldDelete(CachedItem data)
        {
            var currentTime = DateTimeOffset.UtcNow;
            if (data.AbsolutExperiation != null && data.AbsolutExperiation.Value <= currentTime)
            {
                return true;
            }
            if (data.SlidingExperiation.HasValue && data.LastAccessTime.HasValue && data.LastAccessTime <= currentTime.Add(data.SlidingExperiation.Value))
            {
                return true;
            }

            return false;
        }

        public void Remove(string key)
        {
            this.RemoveAsync(key).Wait();
        }

        public Task RemoveAsync(string key)
        {
            var op = TableOperation.Delete(new CachedItem(partitionKey, key));
            return azuretable.ExecuteAsync(op);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            this.SetAsync(key, value, options).Wait();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            DateTimeOffset? absoluteExpiration = null;
            var currentTime = DateTimeOffset.UtcNow;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = currentTime.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else if (options.AbsoluteExpiration.HasValue)
            {
                if (options.AbsoluteExpiration.Value <= currentTime)
                {
                    throw new ArgumentOutOfRangeException(
                       nameof(options.AbsoluteExpiration),
                       options.AbsoluteExpiration.Value,
                       "The absolute expiration value must be in the future.");
                }
                absoluteExpiration = options.AbsoluteExpiration;
            }
            var item = new CachedItem(partitionKey, key, value) { LastAccessTime = currentTime };
            var op = TableOperation.InsertOrReplace(item);
            return this.azuretable.ExecuteAsync(op);
        }
    }
}