using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Desktop.Services;

public class SessionService
{
    public Guid? CurrentUserId { get; private set; }
    public string? Username { get; private set; }
    public List<string> Roles { get; private set; } = new();
    public List<string> Permissions { get; private set; } = new();
    public bool IsAuthenticated => CurrentUserId.HasValue;

    public void SetSession(LoginResponse response)
    {
        CurrentUserId = response.UserId;
        Username = response.Username;
        Roles = response.Roles;
        Permissions = response.Permissions;
    }

    public void Clear()
    {
        CurrentUserId = null;
        Username = null;
        Roles.Clear();
        Permissions.Clear();
    }

    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }

    public bool HasRole(string role)
    {
        return Roles.Contains(role);
    }
}
