# Couchbase Extensions #

[![Build status](https://ci.appveyor.com/api/projects/status/b6oqv14r9jmi8lle/branch/master?svg=true)](https://ci.appveyor.com/project/brantburnett/couchbase-extensions/branch/master)

A collection of extensions for the Couchbase .Net SDK designed to simplify working with Couchbase within .Net Core applications.

## Couchbase.Extensions.DependencyInjection ##

A .Net Core style dependency injection framework for a Couchbase cluster and buckets.  It simplifies cluster configuration, lifetime management, and bucket injection.

### Adding Couchbase To Services ###

The easiest option to to provide the Couchbase configuration as an `IConfiguration` section in your `Startup` class.

```csharp
public IConfigurationRoot Configuration { get; }

public void ConfigureServices(IServiceCollection services)
{
    // Register Couchbase with configuration section
    services.AddCouchbase(Configuration.GetSection("Couchbase"));

    // Register other services, like .AddMvc()
}
```

Alternatively, you can apply manual configuration using an action.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register Couchbase with configuration section
    services.AddCouchbase(clientDefinition =>
    {
        // Set clientDefinition properties here
    });
}
```

### Injecting Couchbase Buckets ###

To get a couchbase bucket, simply inject `IBucketProvider` and call `GetBucket`.  Be sure that you don't dispose the `IBucket`, it's a singleton that will be reused through the application.

```csharp
public class HomeController : Controller
{
    private readonly IBucketProvider _bucketProvider;

    public HomeController(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public IActionResult Index()
    {        
        var bucket = _bucketProvider.GetBucket("bucketname");

        var result =
            await bucket.QueryAsync<Model>(
                "SELECT Extent.* FROM `bucketname` AS Extent");

        if (!result.Success)
        {
            throw new Exception("Couchbase Error", result.Exception);
        }

        return View(result.Rows);
    }
}
```

### Simplifying Injecting Bucket Names ###

To further simplify dependency injection, you can setup to inject specific buckets.  First, create an interface for each bucket that inherits from `INamedBucketProvider`.  This interface must be public and left empty.

```csharp
public interface IMyBucketProvider : INamedBucketProvider
{
}
```

You can then configure your bucket interfaces during `IServiceCollection` setup.

```csharp
public IConfigurationRoot Configuration { get; }

public void ConfigureServices(IServiceCollection services)
{
    // Register Couchbase with configuration section
    services
        .AddCouchbase(Configuration.GetSection("Couchbase"))
        .AddCouchbaseBucket<IMyBucketProvider>("my-bucket");

    // Register other services, like .AddMvc()
}
```

The interface you created can now be injected into controllers or business logic, and the `GetBucket` method will return the specified bucket.  You are no longer required to know the name of the bucket in the controller, improving separation of concerns in your application.

```csharp
public class HomeController : Controller
{
    private readonly IMyBucketProvider _bucketProvider;

    public HomeController(IMyBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public IActionResult Index()
    {        
        var bucket = _bucketProvider.GetBucket();

        var result =
            await bucket.QueryAsync<Model>(
                "SELECT Extent.* FROM `bucketname` AS Extent");

        if (!result.Success)
        {
            throw new Exception("Couchbase Error", result.Exception);
        }

        return View(result.Rows);
    }
}
```

### Shutdown ###

During application shutdown it's best to close the Couchbase connections gracefully.  You can do this using the `ICouchbaseLifetimeService`.  For Asp.Net Core, you can call this service from the ApplicationStopped cancellation token of `IApplicationLifetime`.

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime applicationLifetime)
{
	// Other application startup here

	// When application is stopped gracefully shutdown Couchbase connections
        applicationLifetime.ApplicationStopped.Register(() =>
        {
            app.ApplicationServices.GetRequiredService<ICouchbaseLifetimeService>().Close();
        });
}
```

## Couchbase.Extensions.DnsDiscovery

A .Net Core compatible DNS SRV mechanism for discovering a Couchbase cluster dynamically.

### Configuration

To use, call `AddCouchbaseDnsDiscovery` during the service registration process, usually in your `Startup` class.  You may choose whether or not to add this step based on the environment.  Be sure to add the basic Couchbase configuration first.

```csharp
public class Startup
{
    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();
        Environment = env;
    }

    public IConfigurationRoot Configuration { get; }
    public IHostingEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Register Couchbase with configuration section
        services.AddCouchbase(Configuration.GetSection("Couchbase"));

        if (Environment.IsProduction())
        {
            // Lookup DNS SRV records using the query _couchbase._tcp.services.local
            services.AddCouchbaseDnsDiscovery("_couchbase._tcp.services.local");

           // Note: the record name above could also be retreived from configuration
        }

        // Register other services, like .AddMvc()
    }
```

The above configuration will perform a DNS SRV query (only in the Production environment).  The results will be used to replace the Servers list in the Couchbase client configuration.  Any servers listed in the configuration section will be thrown out.

Note that only the servers with the highest priority in the DNS SRV response will be used.  For example, if the response returns 2 servers with a priority of 10 and 2 different servers with a priority of 20, the first set will be used to initialize the cluster.  SRV record failover is not supported.

Additionally, due to the nature of Couchbase clusters, the weight fields in the DNS SRV records are ignored.  However, the port is used and should normally be set to 8091.
