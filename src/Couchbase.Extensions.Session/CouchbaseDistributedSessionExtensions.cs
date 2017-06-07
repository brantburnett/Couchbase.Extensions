using System;
using Microsoft.AspNetCore.Http;

namespace Couchbase.Extensions.Session
{
    public static class CouchbaseDistributedSessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var couchbaseSession = session as CouchbaseDistributedSession;
            if (couchbaseSession == null)
            {
                throw new NotSupportedException("Only supported by CouchbaseDistributedSession!");
            }

            couchbaseSession.Set(key, value);
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var couchbaseSession = session as CouchbaseDistributedSession;
            if (couchbaseSession == null)
            {
                throw new NotSupportedException("Only supported by CouchbaseDistributedSession!");
            }

            T value;
            couchbaseSession.TryGetValue(key, out value);
            return value;
        }
    }
}
