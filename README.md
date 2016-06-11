# AspNetCache-AzureTableStorage
Use azure table storage for AspNet core 1.0 Distributed Cache.

Azure Table Storage is a very cheap, [super fast](https://www.troyhunt.com/working-with-154-million-records-on/) key value store, and its much cheaper than the redis cluster in azure. This is not a true replacement for redis, and redis should be used if people have money, but this is designed to get people a very cheap cache in azure. Currently this doesn't actually support the dotnet core runtime, and won't until the Azure Storage client is updated to support core.

## How to use

`install-package AzureTableStorageCache`

In your startup.cs


```csharp

 public void ConfigureServices(IServiceCollection services)
        {
            services.AddAzureTableStorageCache("!!!CONNECTIONSTRINGHERE!!!", "tablename", "partitionKey");
            // Add framework services.
            services.AddMvc();
        }


```