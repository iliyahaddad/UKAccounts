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

public class CompanyServiceTests
{
    private async Task<(AppDbContext context, CompanyService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var companyRepository = new Repository<Company>(context);
        var periodRepository = new Repository<AccountingPeriod>(context);

        var logger = NullLogger<CompanyService>.Instance;

        var service = new CompanyService(
            companyRepository,
            periodRepository,
            context,
            logger);

        return (context, service);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateCompany()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCompanyRequest
        {
            CompanyNumber = "12345678",
            CompanyName = "Test Company Ltd",
            AccountingReferenceDate = new DateTime(2025, 12, 31),
            Regime = AccountsRegime.Dormant
        };

        var company = await service.CreateAsync(request);

        Assert.NotNull(company);
        Assert.Equal("12345678", company.CompanyNumber);
        Assert.Equal("Test Company Ltd", company.CompanyName);
        Assert.True(company.IsDormant);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCompany()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCompanyRequest
        {
            CompanyNumber = "12345678",
            CompanyName = "Test Company Ltd",
            AccountingReferenceDate = new DateTime(2025, 12, 31)
        };

        var created = await service.CreateAsync(request);
        var retrieved = await service.GetAsync(created.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Test Company Ltd", retrieved.CompanyName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCompany()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCompanyRequest
        {
            CompanyNumber = "12345678",
            CompanyName = "Test Company Ltd",
            AccountingReferenceDate = new DateTime(2025, 12, 31)
        };

        var created = await service.CreateAsync(request);

        var updateRequest = new CreateCompanyRequest
        {
            CompanyNumber = "87654321",
            CompanyName = "Updated Company Ltd",
            AccountingReferenceDate = new DateTime(2025, 12, 31)
        };

        var updated = await service.UpdateAsync(created.Id, updateRequest);

        Assert.Equal("Updated Company Ltd", updated.CompanyName);
        Assert.Equal("87654321", updated.CompanyNumber);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCompany()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCompanyRequest
        {
            CompanyNumber = "12345678",
            CompanyName = "Test Company Ltd",
            AccountingReferenceDate = new DateTime(2025, 12, 31)
        };

        var created = await service.CreateAsync(request);
        await service.DeleteAsync(created.Id);

        var deleted = await service.GetAsync(created.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task CreateAccountingPeriodAsync_ShouldCreatePeriod()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateCompanyRequest
        {
            CompanyNumber = "12345678",
            CompanyName = "Test Company Ltd",
            AccountingReferenceDate = new DateTime(2025, 12, 31)
        };

        var company = await service.CreateAsync(request);
        var period = await service.CreateAccountingPeriodAsync(company.Id, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));

        Assert.NotNull(period);
        Assert.Equal(company.Id, period.CompanyId);
        Assert.Equal(new DateTime(2025, 1, 1), period.StartDate);
        Assert.Equal(new DateTime(2025, 12, 31), period.EndDate);
    }
}
