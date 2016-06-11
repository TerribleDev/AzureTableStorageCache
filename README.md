# AspNetCache-AzureTableStorage
Use azure table storage for AspNet core 1.0 Distributed Cache.

Azure Table Storage is a very cheap, [super fast](https://www.troyhunt.com/working-with-154-million-records-on/) key value store, and its much cheaper than the redis cluster in azure. This is not a true replacement for redis, and redis should be used if people have money, but this is designed to get people a very cheap cache in azure. Currently this doesn't actually support the dotnet core runtime, and won't until the Azure Storage client is updated to support core.

**warning** As of right now sliding expiration doesn't work. This is at the top of the list to get working

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

Then in a controller just ask for an IDistributedCache in the constructor. Since this implements Microsoft's IDistributed cache, it could be easily swapped out for redis or another Distributed cache.



```csharp

public class HomeController : Controller 
{
        private readonly IDistributedCache cacheMechanism;

        public HomeController(IDistributedCache cacheMechanism)
        {
            this.cacheMechanism = cacheMechanism;
        }
        public async Task<IActionResult> Index()
        {
            var data = await cacheMechanism.GetAsync("awesomeRecord");
            var result = string.Empty;
            if(data != null)
            {
                result = Encoding.UTF32.GetString(data);
            }
            return View(result);

        }

        public async Task<IActionResult> AddCache()
        {
            await cacheMechanism.SetAsync("awesomeRecord", Encoding.UTF32.GetBytes("Im Awesome"));
            ViewData["Message"] = "Your application description page.";

            return RedirectToAction("Index");
        }
}

```