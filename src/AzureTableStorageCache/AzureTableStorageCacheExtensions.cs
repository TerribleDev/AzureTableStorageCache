using AzureTableStorageCache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureTableStorageCacheExtensions
    {
        /// <summary>
        /// Add azure table storage cache as an IDistributedCache to the service container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString">The connection string of your account (can be found in the preview portal)</param>
        /// <param name="tableName">the name of the table you wish to use. If the table doesn't exist it will be created.</param>
        /// <param name="partitionKey">the partition key you would like to use</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureTableStorageCache(this IServiceCollection services, string connectionString, string tableName, string partitionKey)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("connectionString must not be null");
            }

            //services.AddSingleton<Azure>
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, AzureTableStorageCacheHandler>(a => new AzureTableStorageCacheHandler(connectionString, tableName, partitionKey)));
            return services;
        }

        /// <summary>
        /// Add azure table storage cache as an IDistributedCache to the service container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="accountName">the name of your storage account</param>
        /// <param name="accountKey">the key of your storage account</param>
        /// <param name="tableName">the name of the table you wish to use. If the table doesn't exist it will be created.</param>
        /// <param name="partitionKey">the partition key you would like to use</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureTableStorageCache(this IServiceCollection services, string accountName, string accountKey, string tableName, string partitionKey)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                throw new ArgumentNullException("accountName must not be null");
            }
            if (string.IsNullOrWhiteSpace(accountKey))
            {
                throw new ArgumentNullException("accountKey must not be null");
            }

            //services.AddSingleton<Azure>
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, AzureTableStorageCacheHandler>(a =>
            new AzureTableStorageCacheHandler(accountName, accountKey, tableName, partitionKey)));
            return services;
        }

        private static void checkTableData(string tableName, string partitionkey)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("tableName must not be null");
            }
            if (string.IsNullOrWhiteSpace(partitionkey))
            {
                throw new ArgumentNullException("partitionkey must not be null");
            }
        }
    }
}