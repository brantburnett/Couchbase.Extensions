using System;
using System.Collections;
using System.Collections.Generic;
using Couchbase.Core;
using Couchbase.Extensions.Locks.Internal;
using Moq;
using Xunit;

namespace Couchbase.Extensions.Locks.UnitTests.Internal
{
    public class CouchbaseMutexTests
    {
        #region ctor

        public static IEnumerable<object[]> CtorNullParameters()
        {
            var bucket = new Mock<IBucket>().Object;

            yield return new object[] {null, "name", "holder", "bucket"};
            yield return new object[] {bucket, null, "holder", "name"};
            yield return new object[] {bucket, "name", null, "holder"};
        }

        [Theory]
        [MemberData(nameof(CtorNullParameters))]
        public void Ctor_NullParameter_ArgumentNullException(IBucket bucket, string name, string holder, string paramName)
        {
            // Act/Assert

            Assert.Throws<ArgumentNullException>(paramName, () => new CouchbaseMutex(bucket, name, holder));
        }

        #endregion
    }
}
