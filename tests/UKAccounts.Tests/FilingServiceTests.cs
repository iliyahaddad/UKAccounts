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

public class FilingServiceTests
{
    private async Task<(AppDbContext context, FilingService service)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var filingRepository = new Repository<Filing>(context);
        var filingDocumentRepository = new Repository<FilingDocument>(context);
        var filingAttemptRepository = new Repository<FilingAttempt>(context);

        var logger = NullLogger<FilingService>.Instance;

        var service = new FilingService(
            filingRepository,
            filingDocumentRepository,
            filingAttemptRepository,
            context,
            logger);

        return (context, service);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateFiling()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateFilingRequest
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            FilingType = "Accounts",
            GeneratedFileHash = "abc123",
            ValidationResult = "Valid"
        };

        var filing = await service.CreateAsync(request);

        Assert.NotNull(filing);
        Assert.Equal("Accounts", filing.FilingType);
        Assert.Equal("Draft", filing.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateFilingStatus()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateFilingRequest
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            FilingType = "Accounts"
        };

        var filing = await service.CreateAsync(request);
        var updated = await service.UpdateStatusAsync(filing.Id, "Submitted", "Submission successful");

        Assert.Equal("Submitted", updated.Status);
        Assert.Equal("Submission successful", updated.Response);
        Assert.NotNull(updated.SubmissionDate);
    }

    [Fact]
    public async Task AddAttemptAsync_ShouldAddFilingAttempt()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateFilingRequest
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            FilingType = "Accounts"
        };

        var filing = await service.CreateAsync(request);
        var attempt = await service.AddAttemptAsync(filing.Id, "Failed", "Network error");

        Assert.NotNull(attempt);
        Assert.Equal("Failed", attempt.Status);
        Assert.Equal("Network error", attempt.Response);
    }

    [Fact]
    public async Task AddDocumentAsync_ShouldAddFilingDocument()
    {
        using var (context, service) = await CreateContextAsync();

        var request = new CreateFilingRequest
        {
            CompanyId = Guid.Empty,
            AccountingPeriodId = Guid.Empty,
            FilingType = "Accounts"
        };

        var filing = await service.CreateAsync(request);

        var content = System.Text.Encoding.UTF8.GetBytes("<html><body>test</body></html>");
        using var ms = new MemoryStream(content);

        var docRequest = new CreateFilingDocumentRequest
        {
            FileName = "accounts.ixbrl",
            ContentType = "application/xhtml+xml",
            Size = ms.Length,
            Type = DocumentType.Ixbrl
        };

        var document = await service.AddDocumentAsync(filing.Id, docRequest, ms);

        Assert.NotNull(document);
        Assert.Equal("accounts.ixbrl", document.FileName);
        Assert.Equal(DocumentType.Ixbrl, document.Type);
    }
}
