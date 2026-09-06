using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Persistence;
using UKAccounts.Infrastructure.Services;
using Xunit;

namespace UKAccounts.Tests;

public class CustomerServiceTests
{
    private async Task<(AppDbContext context, CustomerService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var customerRepository = new Repository<Customer>(context);
        var logger = NullLogger<CustomerService>.Instance;

        var service = new CustomerService(customerRepository, context, logger);

        return (context, service);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCustomer()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer Ltd",
            Email = "test@example.com"
        };

        var customer = await service.CreateAsync(request);

        Assert.NotNull(customer);
        Assert.Equal("Test Customer Ltd", customer.Name);
        Assert.Equal("test@example.com", customer.Email);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCustomer()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer Ltd"
        };

        var created = await service.CreateAsync(request);
        var retrieved = await service.GetAsync(created.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Test Customer Ltd", retrieved.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCustomer()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer Ltd"
        };

        var created = await service.CreateAsync(request);

        var updateRequest = new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Updated Customer Ltd",
            Email = "updated@example.com"
        };

        var updated = await service.UpdateAsync(created.Id, updateRequest);

        Assert.Equal("Updated Customer Ltd", updated.Name);
        Assert.Equal("updated@example.com", updated.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCustomer()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer Ltd"
        };

        var created = await service.CreateAsync(request);
        await service.DeleteAsync(created.Id);

        var deleted = await service.GetAsync(created.Id);
        Assert.Null(deleted);
    }
}
