using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Security;

namespace UKAccounts.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var found = user.FirstOrDefault(u => u.Username == request.Username || u.Email == request.Username);

        if (found == null)
        {
            _logger.LogWarning("Login failed: user not found {Username}", request.Username);
            return new LoginResponse { Success = false, ErrorMessage = "Invalid username or password." };
        }

        if (!found.IsActive)
        {
            _logger.LogWarning("Login failed: user inactive {UserId}", found.Id);
            return new LoginResponse { Success = false, ErrorMessage = "Account is inactive." };
        }

        if (found.LockedUntil.HasValue && found.LockedUntil > DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Login failed: user locked {UserId}", found.Id);
            return new LoginResponse { Success = false, ErrorMessage = "Account is locked. Try again later." };
        }

        if (!PasswordHasher.Verify(request.Password, found.PasswordHash))
        {
            found.FailedLoginCount++;
            if (found.FailedLoginCount >= 5)
            {
                found.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
                _logger.LogWarning("User locked due to failed logins {UserId}", found.Id);
            }
            await _userRepository.UpdateAsync(found, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new LoginResponse { Success = false, ErrorMessage = "Invalid username or password." };
        }

        found.LastLoginAt = DateTimeOffset.UtcNow;
        found.FailedLoginCount = 0;
        found.LockedUntil = null;
        await _userRepository.UpdateAsync(found, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _userRoleRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        var userRoles = roles.Where(ur => ur.UserId == found.Id).ToList();
        var roleNames = new List<string>();
        var permissions = new HashSet<string>();

        foreach (var userRole in userRoles)
        {
            var role = await _roleRepository.GetByIdAsync(userRole.RoleId, Guid.Empty, cancellationToken);
            if (role != null)
            {
                roleNames.Add(role.Name);
            }
        }

        _logger.LogInformation("User logged in successfully {UserId}", found.Id);
        return new LoginResponse
        {
            Success = true,
            UserId = found.Id,
            Username = found.Username,
            Email = found.Email,
            Roles = roleNames,
            Permissions = permissions.ToList()
        };
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User logged out {UserId}", userId);
        await Task.CompletedTask;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        if (existing.Any(u => u.Username == request.Username || u.Email == request.Email))
        {
            throw new InvalidOperationException("Username or email already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = PasswordHasher.Hash(request.Password),
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var roleId in request.RoleIds)
        {
            await _userRoleRepository.AddAsync(new UserRole { UserId = user.Id, RoleId = roleId }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            Roles = new List<string>()
        };
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, Guid.Empty, cancellationToken);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            Roles = new List<string>()
        };
    }

    public Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
