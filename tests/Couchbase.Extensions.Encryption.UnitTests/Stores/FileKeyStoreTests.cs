using System;
using System.IO;
using Couchbase.Extensions.Encryption.Stores;
using Xunit;
using Xunit.Abstractions;

namespace Couchbase.Extensions.Encryption.UnitTests.Stores
{
    public class FileKeyStoreTests : IDisposable
    {
        private readonly ITestOutputHelper output;
        private readonly string _keyname = "thekeyname";

        public FileKeyStoreTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_StoreKey()
        {
            var keystore = new FileSystemKeyStore();
            keystore.StoreKey(_keyname, "thekeyvalue");
        }

        [Fact]
        public void Test_GetKey()
        {
            Test_StoreKey();
            var keystore = new FileSystemKeyStore();
            var key = keystore.GetKey(_keyname);
            Assert.Equal("thekeyvalue", key);
        }

        public void Dispose()
        {
            try
            {
                File.Delete(_keyname + ".dat");
            }
            catch (Exception e)
            {
                output.WriteLine(e.ToString());
            }
        }
    }
}

#region [ License information          ]
/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2017 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/
#endregion
