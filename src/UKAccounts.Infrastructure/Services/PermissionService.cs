using Microsoft.Extensions.Logging;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IRepository<RolePermission> _rolePermissionRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IRepository<RolePermission> rolePermissionRepository,
        IRepository<Permission> permissionRepository,
        ILogger<PermissionService> logger)
    {
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var permissions = await GetPermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permission);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var userRoleList = userRoles.Where(ur => ur.UserId == userId).ToList();

        var permissions = new HashSet<string>();

        foreach (var userRole in userRoleList)
        {
            var rolePermissions = await _rolePermissionRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
            var rolePerms = rolePermissions.Where(rp => rp.RoleId == userRole.RoleId).ToList();

            foreach (var rp in rolePerms)
            {
                var permission = await _permissionRepository.GetByIdAsync(rp.PermissionId, Guid.Empty, cancellationToken);
                if (permission != null)
                {
                    permissions.Add(permission.Name);
                }
            }
        }

        return permissions.ToList();
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var roles = new List<string>();

        foreach (var userRole in userRoles.Where(ur => ur.UserId == userId))
        {
            var role = await _roleRepository.GetByIdAsync(userRole.RoleId, Guid.Empty, cancellationToken);
            if (role != null)
            {
                roles.Add(role.Name);
            }
        }

        return roles;
    }
}
