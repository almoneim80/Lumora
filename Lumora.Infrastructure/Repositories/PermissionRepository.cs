using Lumora.Application.DTOs;
using Lumora.Application.Interfaces.AuthorizationIntf;

namespace Lumora.Infrastructure.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public PermissionRepository(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<AppRole?> FindRoleByNameAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return null;

            // Mapping IdentityRole to Domain AppRole
            return new AppRole
            {
                Id = role.Id,
                Name = role.Name!,
                NormalizedName = role.NormalizedName,
                ConcurrencyStamp = role.ConcurrencyStamp
            };
        }

        public async Task<IList<Claim>> GetRoleClaimsAsync(AppRole role)
        {
            var identityRole = await _roleManager.FindByIdAsync(role.Id);
            if (identityRole == null) return new List<Claim>();

            return await _roleManager.GetClaimsAsync(identityRole);
        }

        public async Task<OperationResult> AddClaimToRoleAsync(AppRole role, Claim claim)
        {
            var identityRole = await _roleManager.FindByIdAsync(role.Id);
            if (identityRole == null) return OperationResult.Failed("Role not found in identity store.");

            var result = await _roleManager.AddClaimAsync(identityRole, claim);
            return MapIdentityResult(result);
        }

        public async Task<OperationResult> RemoveClaimFromRoleAsync(AppRole role, Claim claim)
        {
            var identityRole = await _roleManager.FindByIdAsync(role.Id);
            if (identityRole == null) return OperationResult.Failed("Role not found in identity store.");

            var result = await _roleManager.RemoveClaimAsync(identityRole, claim);
            return MapIdentityResult(result);
        }

        public async Task<IList<Claim>> GetUserClaimsAsync(User user)
        {
            return await _userManager.GetClaimsAsync(user);
        }

        public async Task<OperationResult> RemoveClaimFromUserAsync(User user, Claim claim)
        {
            var result = await _userManager.RemoveClaimAsync(user, claim);
            return MapIdentityResult(result);
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> RoleHasPermissionAsync(AppRole role, string permission)
        {
            var identityRole = await _roleManager.FindByIdAsync(role.Id);
            if (identityRole == null) return false;

            var claims = await _roleManager.GetClaimsAsync(identityRole);
            return claims.Any(c => c.Type == "Permission" && c.Value == permission);
        }

        public async Task<OperationResult> AddPermissionToRoleAsync(AppRole role, string permission)
        {
            var identityRole = await _roleManager.FindByIdAsync(role.Id);
            if (identityRole == null) return OperationResult.Failed("Role not found.");

            var result = await _roleManager.AddClaimAsync(identityRole, new Claim("Permission", permission));
            return MapIdentityResult(result);
        }

        public async Task<List<string>> GetRoleClaimsValuesByTypeAsync(string roleName, string claimType)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return new List<string>();

            var claims = await _roleManager.GetClaimsAsync(role);
            return claims
                .Where(c => c.Type == claimType)
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Converts Microsoft.AspNetCore.Identity.IdentityResult to Lumora.Application.DTOs.OperationResult.
        /// </summary>
        private OperationResult MapIdentityResult(IdentityResult result)
        {
            return result.Succeeded
                ? OperationResult.Success()
                : OperationResult.Failed(result.Errors.Select(e => e.Description).ToArray());
        }
    }
}
