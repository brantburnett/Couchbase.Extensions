using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core;
using Microsoft.AspNetCore.Identity;

namespace Couchbase.Extensions.Identity
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole> where TRole : IdentityRole
    {
        private IBucket _bucket;

        public RoleStore(IBucket bucket)
        {
            _bucket = bucket;
        }

        public IQueryable<TRole> Roles { get; protected set; }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            var result = await _bucket.InsertAsync(role.Id, role).ConfigureAwait(false);
            if (result.Success)
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(GetIdentityFailure(result));
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            var result = await _bucket.RemoveAsync(role.Id).ConfigureAwait(false);
            if (result.Success)
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(GetIdentityFailure(result));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var result = await _bucket.GetAsync<TRole>(roleId).ConfigureAwait(false);
            return result.Success ? result.Value : null;
        }

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken) =>
            Task.FromResult(role.NormalizedName);

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Id);

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken) => Task.FromResult(role.Name);

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            var result = await _bucket.ReplaceAsync(role.Id, role).ConfigureAwait(false);
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
