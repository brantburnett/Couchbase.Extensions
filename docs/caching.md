# Couchbase Distributed Cache for ASP.NET Core #
A custom ASP.NET Core Middleware plugin for a distributed cache using Couchbase server as the backing store. Supports both Memcached (in-memory) and Couchbase (persistent) buckets.

## Getting Started ##
Assuming you have an [installation of Couchbase Server](https://developer.couchbase.com/documentation/server/4.5/getting-started/installing.html) and Visual Studio (examples with VSCODE forthcoming), do the following:

### Couchbase .NET Core Distributed Cache: ###

- Create a .NET Core Web Application using Visual Studio or VsCodeor CIL
- Install the package from [NuGet](https://www.nuget.org/packages/Couchbase.Extensions.Caching/) or build from source and add reference

### Setup ###
In Setup.cs add the following to the ConfigureServices method:

	public void ConfigureServices(IServiceCollection services)
	{
	    // Add framework services.
	    services.AddMvc();
	
	    services.AddDistributedCouchbaseCache(opt =>
	    {
	        opt.BucketName = "default";
	        opt.Configuration = new ClientConfiguration
	        {
	            Servers = new List<Uri>
	            {
	                new Uri("http://localhost:8091")
	            }
	        };
	        ClusterHelper.Initialize(opt.Configuration);
	        opt.Bucket = ClusterHelper.GetBucket(opt.BucketName);
	    });
	}

  You can change the `localhost` hostname to wherever you are hosting your Couchbase cluster. 

In your controller add a parameter for `IDistributedCache` to the constructor:

	 public class HomeController : Controller
	 {
		private IDistributedCache _cache;
	
	    public HomeController(IDistributedCache cache)
	    {
	    	_cache = cache;
	    }
	 }

### Tear down ###

There are a couple different ways to free up the resources (TCP sockets, etc) opened by the Couchbase `ICluster` and `IBucket` used by the Distributed Session. Here is one simple way to tap into the `ApplicationStopped` cancellation token:

	public void ConfigureServices(IServiceCollection services)
	{
		...

	 	applicationLifetime.ApplicationStopped.Register(() =>
	    {
	        app.ApplicationServices.GetRequiredService<ICouchbaseLifetimeService>().Close();
	    });
	}

### Using Caching in your Controllers ###
 Add the following code to HomeController:

   	public IActionResult Index()
	{
		_cache.Set("CacheTime", System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
		return View();
	}

	public IActionResult About()
	{
		ViewData["Message"] = "Your application description page. "
					+ System.Text.Encoding.UTF8.GetString(_cache.Get("CacheTime"));
		return View();
	}
