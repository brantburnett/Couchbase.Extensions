using System.Threading.Tasks;
using Couchbase.KeyValue;

namespace Couchbase.Extensions.Caching
{
    public interface ICouchbaseCacheCollectionProvider
    {
        ValueTask<ICouchbaseCollection> GetCollectionAsync();
    }
}
