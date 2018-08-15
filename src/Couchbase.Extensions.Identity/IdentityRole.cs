using System;
using System.Collections.Generic;
using System.Text;

namespace Couchbase.Extensions.Identity
{
    public class IdentityRole
    {
        public IdentityRole()
        {
            //Id = ObjectId.GenerateNewId().ToString();
        }

        public IdentityRole(string roleName) : this()
        {
            Name = roleName;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }

        public override string ToString() => Name;
    }
}
