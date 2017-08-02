# Couchbase Distributed Session for ASP.NET Core #
A custom ASP.NET Core Middleware plugin for distributed session state using Couchbase server as the backing store. Supports both Memcached (in-memory) and Couchbase (persistent) buckets.

## Getting Started ##
Assuming you have an [installation of Couchbase Server](https://developer.couchbase.com/documentation/server/4.5/getting-started/installing.html) and Visual Studio (examples with VSCODE forthcoming), do the following:

### Couchbase .NET Core Distributed Cache: ###

- Create a .NET Core Web Application using Visual Studio or VsCode or CIL
- Install the package from [NuGet](https://www.nuget.org/packages/Couchbase.Extensions.Session/) or build from source and add reference

### Setup ###

In Setup.cs add `app.UseSession()` to the `Configure(..)` method to enable session state for the application:

	public void Configure(IApplicationBuilder app, IHostingEnvironment env)
	{
	    app.UseSession();

        ...
	}

In the `ConfigureServices(..)` method add code to initialize the `ICluster` and `IBucket `objects that the Session state will use for persisting application data:

	public void ConfigureServices(IServiceCollection services)
	{
	    // Add framework services.
	    services.AddMvc();
	
	    // Add the couchbase caching service
	    services.AddDistributedCouchbaseCache(opt =>
	    {
	        opt.BucketName = "default";
	        opt.Configuration = new ClientConfiguration
	        {
	            Servers = new List<Uri>
	            {
	                new Uri("http://localhost:8091/")
	            }
	        };
	        ClusterHelper.Initialize(opt.Configuration);
	        opt.Bucket = ClusterHelper.GetBucket(opt.BucketName);
	    });
	
	    services.AddCouchbaseSession(opt =>
	    {
	        opt.IdleTimeout = new TimeSpan(0, 0, 10, 0);
	    });
	}

Note that the Couchbase Distributed Session also uses Couchbase Distributed Cache for persistence. You can use either Couchbase (persistent) or Memcached (in-memory) buckets for the session store.

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

### Using Session in your Controllers ###
To access Session state in your controllers you will use the `Session` property of the static `HttpContext` object. For example, in your `Index` method do the following:

	public IActionResult Index()
	{
	    HttpContext.Session.SetObject("theKey", "{ \"name\" : \"Session stored in couchbase!\"}");
	    return View();
	}

This will store session data with a key of "theKey" into Couchbase server. Note, you can also store POCO's (Plain Ole CSharp Object's) as well and pretty much any other Type using special Extension methods available with the package.

To retrieve the data stored for a key in Session state, you will once again use the `Session` object on the static `HttpContext` object:

	public IActionResult About()
	{
	    ViewData["Message"] = HttpContext.Session.GetObject<string>("theKey");
	    return View();
	}

The `GetObject`is a special Extension method that allows you to read the Session state as the same `Type` that your wrote it to Couchbase as.

To remove an item from the Session store, you will use the `Remove` method and the key used to store the data in Couchbase:

	public IActionResult Contact()
	{
	    HttpContext.Session.Remove("Test");
	    ViewData["Message"] = "Your contact page.";
	    return View();
	}
