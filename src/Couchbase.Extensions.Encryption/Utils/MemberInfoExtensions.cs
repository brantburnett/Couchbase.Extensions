using System;
using System.Reflection;

namespace Couchbase.Extensions.Encryption.Utils
{
    public static class MemberInfoExtensions
    {
        public static bool GetEncryptedFieldAttribute(this MemberInfo methodInfo, out EncryptedFieldAttribute attribute)
        {
#if NETSTANDARD15
            attribute = methodInfo.GetCustomAttribute<EncryptedFieldAttribute>();
            return attribute != null;
#else
            if (Attribute.IsDefined(methodInfo, typeof(EncryptedFieldAttribute)))
            {
                attribute = (EncryptedFieldAttribute)Attribute.
                    GetCustomAttribute(methodInfo, typeof(EncryptedFieldAttribute));

                return true;
            }
            attribute = null;
            return false;
#endif
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
