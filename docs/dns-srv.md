# Couchbase.Extensions.DnsDiscovery

A .Net Core compatible DNS SRV mechanism for discovering a Couchbase cluster dynamically.

## Getting Started ##
Assuming you have an [installation of Couchbase Server](https://developer.couchbase.com/documentation/server/4.5/getting-started/installing.html) and Visual Studio (examples with VSCODE forthcoming), do the following:

- Create a .NET Core Web Application using Visual Studio or VsCodeor CIL
- Install the package from [NuGet](https://www.nuget.org/packages/Couchbase.Extensions.DnsDiscovery/) or build from source and add reference

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
