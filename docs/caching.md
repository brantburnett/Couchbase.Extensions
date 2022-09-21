# Couchbase Distributed Cache for ASP.NET Core #

A custom ASP.NET Core Middleware plugin for a distributed cache using Couchbase server as the backing store. Supports both Ephemeral (in-memory) and Couchbase (persistent) buckets.

## Getting Started ##

Assuming you have an [installation of Couchbase Server](https://docs.couchbase.com/server/current/introduction/intro.html) and Visual Studio (examples with VSCODE forthcoming), do the following:

### Couchbase .NET Core Distributed Cache ###

- Create a .NET Core Web Application using Visual Studio or VsCodeor CIL
- Install the package from [NuGet](https://www.nuget.org/packages/Couchbase.Extensions.Caching/) or build from source and add reference

### Setup ###

In Setup.cs add the following to the ConfigureServices method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add framework services.
    services.AddMvc();

    services.AddCouchbase(opt =>
    {
        opt.ConnectionString = "couchbase://localhost";
        opt.UserName = "Administrator";
        opt.Password = "password";
    });

    services.AddDistributedCouchbaseCache("default", opt => { });
}
```

You can change the `localhost` hostname to wherever you are hosting your Couchbase cluster.

### Using Caching in your Controllers ###

In your controller add a parameter for `IDistributedCache` to the constructor:

```csharp
public class HomeController : Controller
{
    private IDistributedCache _cache;

    public HomeController(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        await _cache.SetAsync("CacheTime", System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
        return View();
    }

    public IActionResult About()
    {
        ViewData["Message"] = "Your application description page. "
                    + System.Text.Encoding.UTF8.GetString(_cache.Get("CacheTime"));
        return View();
    }
}
```

For performance reasons, we strongly recommend using the Async overloads and not the sychronous methods on IDistributeCache.
