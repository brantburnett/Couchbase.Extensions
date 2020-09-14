using System;
using System.Collections.Generic;
using Couchbase.Extensions.Locks.Internal;
using Couchbase.KeyValue;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Couchbase.Extensions.Locks.UnitTests.Internal
{
    public class CouchbaseMutexTests
    {
        #region ctor

        public static IEnumerable<object[]> CtorNullParameters()
        {
            var collection = Mock.Of<ICouchbaseCollection>();

            yield return new object[] {null, "name", "holder", "collection"};
            yield return new object[] {collection, null, "holder", "name"};
            yield return new object[] {collection, "name", null, "holder"};
        }

        [Theory]
        [MemberData(nameof(CtorNullParameters))]
        public void Ctor_NullParameter_ArgumentNullException(ICouchbaseCollection collection, string name, string holder, string paramName)
        {
            // Act/Assert

            Assert.Throws<ArgumentNullException>(paramName, () => new CouchbaseMutex(collection, name, holder, NullLogger<CouchbaseMutex>.Instance));
        }

        #endregion
    }
}
