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

public class BankAccountServiceTests
{
    private async Task<(AppDbContext context, BankAccountService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var bankAccountRepository = new Repository<BankAccount>(context);
        var logger = NullLogger<BankAccountService>.Instance;

        var service = new BankAccountService(bankAccountRepository, context, logger);

        return (context, service);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateBankAccount()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateBankAccountRequest
        {
            CompanyId = Guid.Empty,
            AccountName = "Business Account",
            AccountNumber = "12345678",
            SortCode = "12-34-56",
            Currency = "GBP",
            OpeningBalance = 1000
        };

        var account = await service.CreateAsync(request);

        Assert.NotNull(account);
        Assert.Equal("Business Account", account.AccountName);
        Assert.Equal("12345678", account.AccountNumber);
        Assert.Equal(1000, account.OpeningBalance);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnBankAccount()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateBankAccountRequest
        {
            CompanyId = Guid.Empty,
            AccountName = "Business Account",
            AccountNumber = "12345678",
            SortCode = "12-34-56",
            Currency = "GBP",
            OpeningBalance = 1000
        };

        var created = await service.CreateAsync(request);
        var retrieved = await service.GetAsync(created.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Business Account", retrieved.AccountName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBankAccount()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateBankAccountRequest
        {
            CompanyId = Guid.Empty,
            AccountName = "Business Account",
            AccountNumber = "12345678",
            SortCode = "12-34-56",
            Currency = "GBP",
            OpeningBalance = 1000
        };

        var created = await service.CreateAsync(request);

        var updateRequest = new CreateBankAccountRequest
        {
            CompanyId = Guid.Empty,
            AccountName = "Savings Account",
            AccountNumber = "87654321",
            SortCode = "98-76-54",
            Currency = "GBP",
            OpeningBalance = 5000
        };

        var updated = await service.UpdateAsync(created.Id, updateRequest);

        Assert.Equal("Savings Account", updated.AccountName);
        Assert.Equal("87654321", updated.AccountNumber);
        Assert.Equal(5000, updated.OpeningBalance);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteBankAccount()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateBankAccountRequest
        {
            CompanyId = Guid.Empty,
            AccountName = "Business Account",
            AccountNumber = "12345678",
            SortCode = "12-34-56",
            Currency = "GBP",
            OpeningBalance = 1000
        };

        var created = await service.CreateAsync(request);
        await service.DeleteAsync(created.Id);

        var deleted = await service.GetAsync(created.Id);
        Assert.Null(deleted);
    }
}
