using UKAccounts.Xbrl;
using Xunit;

namespace UKAccounts.Tests;

public class NativeIxbrlGeneratorTests
{
    [Fact]
    public void GenerateAsync_ShouldProduceValidIxbrl()
    {
        var generator = new NativeIxbrlGenerator();
        var request = new IxbrlRequest
        {
            CompanyId = Guid.NewGuid(),
            CompanyNumber = "12345678",
            CompanyName = "Test Company Ltd",
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 12, 31),
            Regime = AccountsRegime.Dormant
        };

        var result = generator.GenerateAsync(request).Result;

        Assert.True(result.Success);
        Assert.NotNull(result.IxbrlContent);
        Assert.Contains("12345678", result.IxbrlContent);
        Assert.Contains("Test Company Ltd", result.IxbrlContent);
        Assert.Contains("inlineXBRL", result.IxbrlContent);
    }

    [Fact]
    public void GenerateAsync_ShouldIncludeRequiredTags()
    {
        var generator = new NativeIxbrlGenerator();
        var request = new IxbrlRequest
        {
            CompanyId = Guid.NewGuid(),
            CompanyNumber = "12345678",
            CompanyName = "Test Company Ltd",
            PeriodStart = new DateTime(2025, 1, 1),
            PeriodEnd = new DateTime(2025, 12, 31),
            Regime = AccountsRegime.Dormant
        };

        var result = generator.GenerateAsync(request).Result;

        Assert.True(result.Success);
        Assert.Contains("uk-gaap:NameOfEntity", result.IxbrlContent);
        Assert.Contains("uk-gaap:CompanyRegistrationNumber", result.IxbrlContent);
        Assert.Contains("uk-gaap:EntityLegalForm", result.IxbrlContent);
    }
}
