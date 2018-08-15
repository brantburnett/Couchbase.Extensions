using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Couchbase.Extensions.Identity
{
    /// <summary>
    /// A claim that a user possesses.
    /// </summary>
    public class IdentityUserClaim
    {
        public IdentityUserClaim()
        {
        }

        public IdentityUserClaim(Claim claim)
        {
            Type = claim.Type;
            Value = claim.Value;
        }

        /// <summary>
        /// Claim type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Claim value
        /// </summary>
        public string Value { get; set; }

        public Claim ToSecurityClaim()
        {
            return new Claim(Type, Value);
        }
    }
}
