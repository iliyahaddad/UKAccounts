using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UKAccounts.Domain.Entities;
using UKAccounts.Infrastructure.Persistence;
using Xunit;

namespace UKAccounts.IntegrationTests;

public class PersistenceTests
{
    [Fact]
    public async Task CanInsertAndRetrieveCompany()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var company = new Company
        {
            CompanyNumber = "00000000",
            CompanyName = "TEST COMPANY LIMITED",
            AccountingReferenceDate = new DateTime(2025, 12, 31)
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var retrieved = await context.Companies.FirstOrDefaultAsync(c => c.CompanyNumber == "00000000");
        Assert.NotNull(retrieved);
        Assert.Equal("TEST COMPANY LIMITED", retrieved.CompanyName);
    }
}
