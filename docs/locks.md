# Couchbase.Extensions.Locks

A system for managing distributed mutexs backed by Couchbase. This can prevent multiple simultaneous processes in separate application instances, which is useful for microservices or other horizontally scaled architectures.

## Getting Started

Assuming you have an [installation of Couchbase Server](https://docs.couchbase.com/server/current/introduction/intro.html) and Visual Studio  VSCODE forthcoming), do the following:

- Install the package from [NuGet](https://www.nuget.org/packages/Couchbase.Extensions.Locks/) or build from source and add a reference.

## Example Application

There is an example MVC application in the [examples directory](../example).

## Requesting a Mutex

To request a Mutex, use the `RequestMutex` extension method on `IBucket`. This can be any type of bucket, but Memcached or Ephemeral would be the most efficient since they never write to disk.

The lock expiration controls when the lock will expire if it isn't explicitly released. The mutex may also be disposed to release the lock early. If the lock cannot be acquired because another process is holding the lock, a `CouchbaseLockUnavailableException` will be thrown.

```csharp
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Extensions.Locks;
using Microsoft.AspNetCore.Mvc;

public class MyController : Controller
{
    private readonly IBucketProvider _bucketProvider;

    public MyController(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public IActionResult Index()
    {
        var bucket = await _bucketProvider.GetBucketAsync("default");
        var collection = bucket.DefaultCollection();

        try {
            using (var mutex = await collection.RequestMutexAsync("my-lock-name", TimeSpan.FromSeconds(15)))
            {
                // This lock will be held for the shorter of the using statement lifespan or 15 seconds
            }
        }
        catch (CouchbaseLockUnavailableException ex)
        {
            // This exception indicates the lock is already held by another process
        }
    }
}
```

## Renewing the lock

If your process is long-running, the lock may be renewed before it expires. This will succeed so long as the lock hasn't been released and acquired by another process. If the lock already expired but is still released, it will be automatically reacquired.

```csharp
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Extensions.Locks;
using Microsoft.AspNetCore.Mvc;

public class MyController : Controller
{
    private readonly IBucketProvider _bucketProvider;

    public MyController(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public IActionResult Index()
    {
        var bucket = await _bucketProvider.GetBucketAsync("default");
        var collection = bucket.DefaultCollection();

        try {
            using (var mutex = await collection.RequestMutexAsync("my-lock-name", TimeSpan.FromSeconds(15)))
            {
                while (true)
                {
                    // Do some work here

                    // This lock will be held until the end of the using statement,
                    // unless one of the loop iterations takes longer than 15 seconds.

                    await mutex.RenewAsync(TimeSpan.FromSeconds(15));
                }
            }
        }
        catch (CouchbaseLockUnavailableException ex)
        {
            // This exception indicates the lock is already held by another process
        }
    }
}
```

## Automatically Renewing Locks

In many cases, it may be desirable for locks to automatically renew themselves until they expire, allowing a shorter expiration time. This means that if the process crashes the lock is freed soon, but the lock can constantly be refreshed so long as the process is still running.

This feature is enabled by calling `AutoRenew` on the mutex after it is acquired. The first parameter is how often to renew the lock, and should be set to significantly shorter than the original lock expiration.

The second parameter is the maximum lifetime of the lock. This is a safety mechanism in case the call to Dispose is somehow missed. Auto renewal will cease after this amount of time.

```csharp
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Extensions.Locks;
using Microsoft.AspNetCore.Mvc;

public class MyController : Controller
{
    private readonly IBucketProvider _bucketProvider;

    public MyController(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public IActionResult Index()
    {
        var bucket = await _bucketProvider.GetBucketAsync("default");
        var collection = bucket.DefaultCollection();

        try {
            using (var mutex = await collection.RequestMutexAsync("my-lock-name", TimeSpan.FromSeconds(10)))
            {
                mutex.AutoRenew(TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));

                while (true)
                {
                    // Do some work here

                    // This lock will be held until the end of the using statement,
                    // so long as the total loop time is less than one minute
                }
            }
        }
        catch (CouchbaseLockUnavailableException ex)
        {
            // This exception indicates the lock is already held by another process
        }
    }
}
```

## Retrying Locks

By design, requesting a mutex is only attempted once. If the lock is unavailable, `CouchbaseLockUnavailableException` is thrown immediately. This allows the consumer to implement advanced retry logic. We recommend using [Polly](https://github.com/App-vNext/Polly) or a similar library. This allows waits, exponential backoffs, circuit breakers, bulkhead isolation, and other advanced logic.

```csharp
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Extensions.Locks;
using Microsoft.AspNetCore.Mvc;
using Polly;

public class MyController : Controller
{
    // Retry up to 10 times, waiting one second between attempts
    private static readonly Policy LockPolicy =
        Policy.Handle<CouchbaseLockUnavailableException>()
            .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(1));

    private readonly IBucketProvider _bucketProvider;

    public MyController(IBucketProvider bucketProvider)
    {
        _bucketProvider = bucketProvider;
    }

    public IActionResult Index()
    {
        var bucket = await _bucketProvider.GetBucketAsync("default");
        var collection = bucket.DefaultCollection();

        try {
            // Wrapping the call in LockPolicy.ExecuteAsync applies the wait and retry logic
            // for any CouchbaseLockUnavailableException.  All other exceptions throw immediately.
            using (var mutex = await LockPolicy.ExecuteAsync(
                () => collection.RequestMutexAsync("my-lock-name", TimeSpan.FromSeconds(10))))
            {
                // Do work here
            }
        }
        catch (CouchbaseLockUnavailableException ex)
        {
            // This exception indicates the lock is already held by another process,
            // for 10 consequetive lock attempts
        }
    }
}
```
