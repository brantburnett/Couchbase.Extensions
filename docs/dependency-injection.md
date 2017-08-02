## Couchbase.Extensions.DependencyInjection ##

A .Net Core style dependency injection framework for a Couchbase cluster and buckets.  It simplifies cluster configuration, lifetime management, and bucket injection.

## Getting Started ##
Assuming you have an [installation of Couchbase Server](https://developer.couchbase.com/documentation/server/4.5/getting-started/installing.html) and Visual Studio (examples with VSCODE forthcoming), do the following:

- Create a .NET Core Web Application using Visual Studio or VsCodeor CIL
- Install the package from [NuGet](https://www.nuget.org/packages/Couchbase.Extensions.DependencyInjection/) or build from source and add reference

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
