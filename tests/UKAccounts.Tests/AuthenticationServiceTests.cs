using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Persistence;
using UKAccounts.Infrastructure.Services;
using Xunit;

namespace UKAccounts.Tests;

public class AuthenticationServiceTests
{
    private async Task<(AppDbContext context, AuthenticationService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var userRepository = new Repository<User>(context);
        var roleRepository = new Repository<Role>(context);
        var userRoleRepository = new Repository<UserRole>(context);

        var logger = NullLogger<AuthenticationService>.Instance;

        var service = new AuthenticationService(
            userRepository,
            roleRepository,
            userRoleRepository,
            context,
            logger);

        return (context, service);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldSucceed()
    {
        using var (context, service) = await CreateContextAsync();

        var createRequest = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPass123!"
        };

        await service.CreateUserAsync(createRequest);

        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "TestPass123!"
        };

        var result = await service.LoginAsync(loginRequest);

        Assert.True(result.Success);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldFail()
    {
        using var (context, service) = await CreateContextAsync();

        var createRequest = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "TestPass123!"
        };

        await service.CreateUserAsync(createRequest);

        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        var result = await service.LoginAsync(loginRequest);

        Assert.False(result.Success);
        Assert.Equal("Invalid username or password.", result.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldFail()
    {
        using var (context, service) = await CreateContextAsync();

        var loginRequest = new LoginRequest
        {
            Username = "nonexistent",
            Password = "password"
        };

        var result = await service.LoginAsync(loginRequest);

        Assert.False(result.Success);
    }
}
