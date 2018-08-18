using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Identity;

namespace Couchbase.Extensions.Identity
{
    public class RoleStore : RoleStore<IdentityRole>
    {
        public RoleStore(ICouchbaseIdentityBucketProvider provider) : base(provider)
        {
        }
    }

    public class RoleStore<TRole> : IQueryableRoleStore<TRole> where TRole : IdentityRole
    {
        private IBucketContext _context;

        public RoleStore(ICouchbaseIdentityBucketProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _context = new BucketContext(provider.GetBucket());
        }

        public IQueryable<TRole> Roles { get; protected set; }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (role.Id == null)
            {
                role.Id = Guid.NewGuid().ToString();
            }
            var result = await _context.Bucket.InsertAsync(role.Id, role).ConfigureAwait(false);
            if (result.Success)
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(GetIdentityFailure(result));
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var result = await _context.Bucket.RemoveAsync(role.Id).ConfigureAwait(false);
            if (result.Success)
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(GetIdentityFailure(result));
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (roleId == null)
            {
                throw new ArgumentNullException(nameof(roleId));
            }

            var result = await _context.Bucket.GetAsync<TRole>(roleId).ConfigureAwait(false);
            return result.Success ? result.Value : null;
        }

        public async Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var query = await (from r in _context.Query<TRole>()
                where r.NormalizedName == normalizedRoleName
                select r).Take(1).ExecuteAsync(cancellationToken).ConfigureAwait(false);

            return query.SingleOrDefault();
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var result = await _context.Bucket.ReplaceAsync(role.Id, role).ConfigureAwait(false);
            if (result.Success)
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(GetIdentityFailure(result));
        }

        private IdentityError GetIdentityFailure(IOperationResult operationResult)
        {
            return new IdentityError
            {
                Code = operationResult.Status.ToString(),
                Description = operationResult.Exception?.ToString()
            };
        }
    }
}
