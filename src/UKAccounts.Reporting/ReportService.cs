using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Accounting;

namespace UKAccounts.Reporting;

public class ReportService : IReportService
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<JournalLine> _lineRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<BankTransaction> _bankTransactionRepository;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IRepository<Account> accountRepository,
        IRepository<Journal> journalRepository,
        IRepository<JournalLine> lineRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<Bill> billRepository,
        IRepository<BankTransaction> bankTransactionRepository,
        ILogger<ReportService> logger)
    {
        _accountRepository = accountRepository;
        _journalRepository = journalRepository;
        _lineRepository = lineRepository;
        _invoiceRepository = invoiceRepository;
        _billRepository = billRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _logger = logger;
    }

    public async Task<TrialBalanceReport> GetTrialBalanceAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);

        var postedJournals = journals
            .Where(j => j.Status == JournalStatus.Posted && j.Date >= from && j.Date <= to)
            .ToList();

        var postedLines = lines
            .Where(l => postedJournals.Select(j => j.Id).Contains(l.JournalId))
            .ToList();

        var accountDict = accounts.ToDictionary(a => a.Id);

        var trialBalance = postedLines
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var account = accountDict.TryGetValue(g.Key, out var acc) ? acc : null;
                return new TrialBalanceDto
                {
                    AccountId = g.Key,
                    AccountCode = account?.Code ?? string.Empty,
                    AccountName = account?.Name ?? "Unknown",
                    Category = account?.Category.ToString() ?? "Unknown",
                    Debit = g.Sum(l => l.Debit),
                    Credit = g.Sum(l => l.Credit)
                };
            })
            .OrderBy(t => t.AccountCode)
            .ToList();

        return new TrialBalanceReport
        {
            CompanyId = companyId,
            FromDate = from,
            ToDate = to,
            Lines = trialBalance
        };
    }

    public async Task<ProfitLossReport> GetProfitLossAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);

        var postedJournals = journals
            .Where(j => j.Status == JournalStatus.Posted && j.Date >= from && j.Date <= to)
            .ToList();

        var postedLines = lines
            .Where(l => postedJournals.Select(j => j.Id).Contains(l.JournalId))
            .ToList();

        var accountDict = accounts.ToDictionary(a => a.Id);

        var incomeLines = postedLines
            .Where(l => accountDict.TryGetValue(l.AccountId, out var acc) && acc.Type == AccountType.Income)
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var account = accountDict[g.Key];
                return new ProfitLossLine
                {
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Category = account.Category.ToString(),
                    Amount = g.Sum(l => l.Credit) - g.Sum(l => l.Debit)
                };
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        var expenseLines = postedLines
            .Where(l => accountDict.TryGetValue(l.AccountId, out var acc) && acc.Type == AccountType.Expense)
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var account = accountDict[g.Key];
                return new ProfitLossLine
                {
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Category = account.Category.ToString(),
                    Amount = g.Sum(l => l.Debit) - g.Sum(l => l.Credit)
                };
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        var report = new ProfitLossReport
        {
            CompanyId = companyId,
            FromDate = from,
            ToDate = to,
            Sections = new List<ProfitLossSection>
            {
                new ProfitLossSection
                {
                    Title = "Income",
                    Type = ReportSectionType.Revenue,
                    Lines = incomeLines
                },
                new ProfitLossSection
                {
                    Title = "Expenses",
                    Type = ReportSectionType.Expense,
                    Lines = expenseLines
                }
            }
        };

        return report;
    }

    public async Task<BalanceSheetReport> GetBalanceSheetAsync(Guid companyId, DateTime to, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);

        var postedJournals = journals
            .Where(j => j.Status == JournalStatus.Posted && j.Date <= to)
            .ToList();

        var postedLines = lines
            .Where(l => postedJournals.Select(j => j.Id).Contains(l.JournalId))
            .ToList();

        var accountDict = accounts.ToDictionary(a => a.Id);

        var assetLines = postedLines
            .Where(l => accountDict.TryGetValue(l.AccountId, out var acc) && acc.Type == AccountType.Asset)
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var account = accountDict[g.Key];
                return new BalanceSheetLine
                {
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Amount = g.Sum(l => l.Debit) - g.Sum(l => l.Credit)
                };
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        var liabilityLines = postedLines
            .Where(l => accountDict.TryGetValue(l.AccountId, out var acc) && acc.Type == AccountType.Liability)
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var account = accountDict[g.Key];
                return new BalanceSheetLine
                {
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Amount = g.Sum(l => l.Credit) - g.Sum(l => l.Debit)
                };
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        var equityLines = postedLines
            .Where(l => accountDict.TryGetValue(l.AccountId, out var acc) && acc.Type == AccountType.Equity)
            .GroupBy(l => l.AccountId)
            .Select(g =>
            {
                var account = accountDict[g.Key];
                return new BalanceSheetLine
                {
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    Amount = g.Sum(l => l.Credit) - g.Sum(l => l.Debit)
                };
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        return new BalanceSheetReport
        {
            CompanyId = companyId,
            AsOfDate = to,
            Sections = new List<BalanceSheetSection>
            {
                new BalanceSheetSection { Title = "Assets", Type = BalanceSheetSectionType.Assets, Lines = assetLines },
                new BalanceSheetSection { Title = "Liabilities", Type = BalanceSheetSectionType.Liabilities, Lines = liabilityLines },
                new BalanceSheetSection { Title = "Equity", Type = BalanceSheetSectionType.Equity, Lines = equityLines }
            }
        };
    }

    public async Task<GeneralLedgerReport> GetGeneralLedgerAsync(Guid companyId, Guid accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, companyId, cancellationToken);
        if (account == null) throw new InvalidOperationException("Account not found.");

        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);

        var postedJournals = journals
            .Where(j => j.Status == JournalStatus.Posted && j.Date >= from && j.Date <= to)
            .ToList();

        var accountLines = lines
            .Where(l => l.AccountId == accountId && postedJournals.Select(j => j.Id).Contains(l.JournalId))
            .OrderBy(l => journals.First(j => j.Id == l.JournalId).Date)
            .ToList();

        var openingBalance = CalculateOpeningBalance(accountId, from, journals, lines);

        var entries = accountLines.Select(l => new GeneralLedgerEntry
        {
            Date = journals.First(j => j.Id == l.JournalId).Date,
            JournalReference = journals.First(j => j.Id == l.JournalId).Reference,
            Description = l.Description,
            Debit = l.Debit,
            Credit = l.Credit
        }).ToList();

        return new GeneralLedgerReport
        {
            CompanyId = companyId,
            AccountId = accountId,
            AccountCode = account.Code,
            AccountName = account.Name,
            FromDate = from,
            ToDate = to,
            OpeningBalance = openingBalance,
            Entries = entries
        };
    }

    public async Task<CashFlowReport> GetCashFlowAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var journals = await _journalRepository.GetByCompanyAsync(companyId, cancellationToken);
        var lines = await _lineRepository.GetByCompanyAsync(companyId, cancellationToken);
        var transactions = await _bankTransactionRepository.GetByCompanyAsync(companyId, cancellationToken);

        var postedJournals = journals
            .Where(j => j.Status == JournalStatus.Posted && j.Date >= from && j.Date <= to)
            .ToList();

        var postedLines = lines
            .Where(l => postedJournals.Select(j => j.Id).Contains(l.JournalId))
            .ToList();

        var bankTransactions = transactions
            .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
            .ToList();

        return new CashFlowReport
        {
            CompanyId = companyId,
            FromDate = from,
            ToDate = to,
            Sections = new List<CashFlowSection>
            {
                new CashFlowSection
                {
                    Title = "Operating Activities",
                    Type = CashFlowType.Operating,
                    Lines = new List<CashFlowLine>
                    {
                        new CashFlowLine { Description = "Net Cash from Operations", Amount = bankTransactions.Sum(t => t.Amount) }
                    }
                }
            }
        };
    }

    public async Task<AgedReceivablesReport> GetAgedReceivablesAsync(Guid companyId, DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetByCompanyAsync(companyId, cancellationToken);
        var outstandingInvoices = invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .ToList();

        var lines = outstandingInvoices.Select(invoice =>
        {
            var daysOverdue = (asOfDate - invoice.DueDate).Days;
            var current = 0m;
            var days1to30 = 0m;
            var days31to60 = 0m;
            var days61to90 = 0m;
            var over90 = 0m;

            if (daysOverdue <= 0)
            {
                current = invoice.Total;
            }
            else if (daysOverdue <= 30)
            {
                days1to30 = invoice.Total;
            }
            else if (daysOverdue <= 60)
            {
                days31to60 = invoice.Total;
            }
            else if (daysOverdue <= 90)
            {
                days61to90 = invoice.Total;
            }
            else
            {
                over90 = invoice.Total;
            }

            return new AgedReceivableLine
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.Customer?.Name ?? string.Empty,
                InvoiceDate = invoice.Date,
                DueDate = invoice.DueDate,
                Current = current,
                Days1to30 = days1to30,
                Days31to60 = days31to60,
                Days61to90 = days61to90,
                Over90 = over90
            };
        }).ToList();

        return new AgedReceivablesReport
        {
            CompanyId = companyId,
            AsOfDate = asOfDate,
            Lines = lines
        };
    }

    public async Task<AgedPayablesReport> GetAgedPayablesAsync(Guid companyId, DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetByCompanyAsync(companyId, cancellationToken);
        var outstandingBills = bills
            .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Cancelled)
            .ToList();

        var lines = outstandingBills.Select(bill =>
        {
            var daysOverdue = (asOfDate - bill.DueDate).Days;
            var current = 0m;
            var days1to30 = 0m;
            var days31to60 = 0m;
            var days61to90 = 0m;
            var over90 = 0m;

            if (daysOverdue <= 0)
            {
                current = bill.Total;
            }
            else if (daysOverdue <= 30)
            {
                days1to30 = bill.Total;
            }
            else if (daysOverdue <= 60)
            {
                days31to60 = bill.Total;
            }
            else if (daysOverdue <= 90)
            {
                days61to90 = bill.Total;
            }
            else
            {
                over90 = bill.Total;
            }

            return new AgedPayableLine
            {
                BillId = bill.Id,
                BillNumber = bill.BillNumber,
                SupplierName = bill.Supplier?.Name ?? string.Empty,
                BillDate = bill.Date,
                DueDate = bill.DueDate,
                Current = current,
                Days1to30 = days1to30,
                Days31to60 = days31to60,
                Days61to90 = days61to90,
                Over90 = over90
            };
        }).ToList();

        return new AgedPayablesReport
        {
            CompanyId = companyId,
            AsOfDate = asOfDate,
            Lines = lines
        };
    }

    private decimal CalculateOpeningBalance(Guid accountId, DateTime fromDate, List<Journal> journals, List<JournalLine> lines)
    {
        var beforeJournals = journals
            .Where(j => j.Status == JournalStatus.Posted && j.Date < fromDate)
            .ToList();

        var beforeLines = lines
            .Where(l => l.AccountId == accountId && beforeJournals.Select(j => j.Id).Contains(l.JournalId))
            .ToList();

        var account = _accountRepository.GetByIdAsync(accountId, Guid.Empty).Result;
        if (account == null) return 0;

        return account.Type switch
        {
            AccountType.Asset => beforeLines.Sum(l => l.Debit) - beforeLines.Sum(l => l.Credit),
            AccountType.Liability => beforeLines.Sum(l => l.Credit) - beforeLines.Sum(l => l.Debit),
            AccountType.Equity => beforeLines.Sum(l => l.Credit) - beforeLines.Sum(l => l.Debit),
            AccountType.Income => beforeLines.Sum(l => l.Credit) - beforeLines.Sum(l => l.Debit),
            AccountType.Expense => beforeLines.Sum(l => l.Debit) - beforeLines.Sum(l => l.Credit),
            _ => 0
        };
    }
}
