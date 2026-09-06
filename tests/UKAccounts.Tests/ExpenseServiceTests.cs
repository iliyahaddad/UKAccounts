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

public class ExpenseServiceTests
{
    private async Task<(AppDbContext context, ExpenseService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var expenseRepository = new Repository<Expense>(context);
        var logger = NullLogger<ExpenseService>.Instance;

        var service = new ExpenseService(expenseRepository, context, logger);

        return (context, service);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateExpense()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateExpenseRequest
        {
            CompanyId = Guid.Empty,
            Date = DateTime.Today,
            Category = "Travel",
            Description = "Train ticket",
            Amount = 50,
            PaymentAccount = "Bank"
        };

        var expense = await service.CreateAsync(request);

        Assert.NotNull(expense);
        Assert.Equal("Travel", expense.Category);
        Assert.Equal("Train ticket", expense.Description);
        Assert.Equal(50, expense.Amount);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnExpense()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateExpenseRequest
        {
            CompanyId = Guid.Empty,
            Date = DateTime.Today,
            Category = "Travel",
            Description = "Train ticket",
            Amount = 50,
            PaymentAccount = "Bank"
        };

        var created = await service.CreateAsync(request);
        var retrieved = await service.GetAsync(created.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Train ticket", retrieved.Description);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExpense()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateExpenseRequest
        {
            CompanyId = Guid.Empty,
            Date = DateTime.Today,
            Category = "Travel",
            Description = "Train ticket",
            Amount = 50,
            PaymentAccount = "Bank"
        };

        var created = await service.CreateAsync(request);

        var updateRequest = new CreateExpenseRequest
        {
            CompanyId = Guid.Empty,
            Date = DateTime.Today,
            Category = "Travel",
            Description = "Flight ticket",
            Amount = 150,
            PaymentAccount = "Bank"
        };

        var updated = await service.UpdateAsync(created.Id, updateRequest);

        Assert.Equal("Flight ticket", updated.Description);
        Assert.Equal(150, updated.Amount);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteExpense()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateExpenseRequest
        {
            CompanyId = Guid.Empty,
            Date = DateTime.Today,
            Category = "Travel",
            Description = "Train ticket",
            Amount = 50,
            PaymentAccount = "Bank"
        };

        var created = await service.CreateAsync(request);
        await service.DeleteAsync(created.Id);

        var deleted = await service.GetAsync(created.Id);
        Assert.Null(deleted);
    }
}
