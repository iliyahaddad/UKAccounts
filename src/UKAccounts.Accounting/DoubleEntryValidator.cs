namespace UKAccounts.Accounting;

public static class DoubleEntryValidator
{
    public static bool IsBalanced(IEnumerable<JournalLine> lines)
    {
        var totalDebits = lines.Sum(l => l.Debit);
        var totalCredits = lines.Sum(l => l.Credit);
        return totalDebits == totalCredits;
    }
}
