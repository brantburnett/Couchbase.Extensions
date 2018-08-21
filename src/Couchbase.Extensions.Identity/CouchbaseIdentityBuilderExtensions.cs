using System;
using System.Collections.Generic;
using System.Text;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Couchbase.Extensions.Identity
{
    public static class CouchbaseIdentityBuilderExtensions
    {
        public static IdentityBuilder RegisterCouchbaseStores<TUser, TRole>(this IdentityBuilder builder,
            IConfiguration configuration, string bucketName)
            where TUser : IdentityUser
            where TRole : IdentityRole
        {
            builder.Services.AddCouchbase(configuration);
            builder.Services.AddCouchbaseBucket<ICouchbaseIdentityBucketProvider>(bucketName);

            builder.AddRoleStore<RoleStore<TRole>>();
            builder.AddUserStore<UserStore<TUser>>();

            return builder;
        }

        public static IdentityBuilder AddCouchbaseIdentity<TUser, TRole>(this IServiceCollection services, IConfiguration configuration, string bucketName)
            where TUser : IdentityUser, new()
            where TRole : IdentityRole, new()
        {
            return services.AddIdentity<TUser, TRole>()
                .RegisterCouchbaseStores<TUser, TRole>(configuration, bucketName)
                .AddDefaultTokenProviders();
        }
    }
}
