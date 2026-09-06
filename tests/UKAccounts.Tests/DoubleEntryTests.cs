using UKAccounts.Domain.Entities;
using UKAccounts.Accounting;
using Xunit;

namespace UKAccounts.Tests;

public class DoubleEntryTests
{
    [Fact]
    public void BalancedJournal_ShouldBeValid()
    {
        var lines = new List<JournalLine>
        {
            new JournalLine { Debit = 100, Credit = 0 },
            new JournalLine { Debit = 0, Credit = 100 }
        };
        Assert.True(DoubleEntryValidator.IsBalanced(lines));
    }

    [Fact]
    public void UnbalancedJournal_ShouldBeInvalid()
    {
        var lines = new List<JournalLine>
        {
            new JournalLine { Debit = 100, Credit = 0 },
            new JournalLine { Debit = 0, Credit = 50 }
        };
        Assert.False(DoubleEntryValidator.IsBalanced(lines));
    }
}
