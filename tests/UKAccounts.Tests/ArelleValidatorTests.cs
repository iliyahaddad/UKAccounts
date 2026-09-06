using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Infrastructure.Services;
using Xunit;

namespace UKAccounts.Tests;

public class ArelleValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldReturnValidResultForExistingFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.html");
        await File.WriteAllTextAsync(tempFile, "<html><body>test</body></html>");

        try
        {
            var logger = NullLogger<ArelleValidator>.Instance;
            var validator = new ArelleValidator(logger);
            var result = await validator.ValidateAsync(tempFile);

            Assert.NotNull(result);
            Assert.IsType<ArelleValidationResult>(result);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnErrorForMissingFile()
    {
        var logger = NullLogger<ArelleValidator>.Instance;
        var validator = new ArelleValidator(logger);
        var result = await validator.ValidateAsync("nonexistent_file.html");

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("FILE_NOT_FOUND", result.Errors[0].Code);
    }

    [Fact]
    public async Task ValidateAsync_ShouldIncludeDuration()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.html");
        await File.WriteAllTextAsync(tempFile, "<html><body>test</body></html>");

        try
        {
            var logger = NullLogger<ArelleValidator>.Instance;
            var validator = new ArelleValidator(logger);
            var result = await validator.ValidateAsync(tempFile);

            Assert.NotNull(result);
            Assert.True(result.Duration >= TimeSpan.Zero);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
