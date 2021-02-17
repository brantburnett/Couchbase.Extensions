# Couchbase Compression using Snappier

Using compression for key/value operations can improve performance by reducing the amount of data sent and
received over the network. It is trading CPU cycles for reducing network utilization. In some cloud
environments it may also reduce network-related costs.

For more details, see <https://docs.couchbase.com/server/current/learn/buckets-memory-and-storage/compression.html>.

The Couchbase .NET SDK 3.x doesn't include built-in support for compression. This is because Couchbase uses
[Snappy](https://github.com/google/snappy) for its compression algorithm, which historically hasn't had a
great cross-platform implementation in .NET. Since version 3.1, the Couchbase .NET SDK does support
extensibility points to add an external implementation of Snappy.

## Couchbase.Extensions.Compression.Snappier

The Couchbase.Extensions.Compression.Snappier is a simple adapter library that wires the Couchbase SDK to
the [Snappier](https://github.com/brantburnett/Snappier) library. This is a .NET port of the C++ Snappy
compression algorithm. It uses C# only, without P/Invoke to native code, to maintain full compatibilty with
all CPU architectures and operating systems. In order to deliver near C++ performance, it uses pointer
arithmetic and hardware intrinsics.

Due to the high performance requirements of Couchbase key/value operations, it is recommend that this
adapter only be used with .NET Core 3.1 or .NET 5.0 and later. Earlier versions of the .NET runtime do not
support the required hardware intrinsics to get all of the performance benefits. It is also recommended
that the application be run in x64 on an Intel or AMD processor, not ARM. However, older versions of the
.NET runtime and other architectures like x86 and ARM are supported, they will just have a CPU performance
penalty.

### Configuring Snappier

To enable Snappier compression, simply call `WithSnappierCompression` on your `ClusterOptions` object
before connecting your cluster.

```cs
var options = new ClusterOptions()
    .WithCredentials("Administrator", "password")
    .WithSnappierCompression();

var cluster = await Cluster.ConnectAsync("couchbase://localhost", options);
```

This will negotiate compression with the server during bootstrap and apply compression to any requests
or responses which support it.

The compression settings may be further configured if advanced tuning is required:

```cs
options.Compression = true;          // Default is true, setting to false disables even if WithSnappierCompression is called
options.CompressionMinRatio = 0.90f; // Default is 0.83 == 83%, the compressed result is discarded if this ratio is not acheived
options.CompressionMinSize = 128;    // Default is 32 bytes, docs smaller than this never try to compress on mutations
```
