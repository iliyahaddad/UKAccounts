using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.UK;

public class MicroEntityAccountsGenerator : IAccountsGenerator
{
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<AccountingPeriod> _periodRepository;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<BankTransaction> _bankTransactionRepository;
    private readonly ILogger<MicroEntityAccountsGenerator> _logger;

    public MicroEntityAccountsGenerator(
        IRepository<Company> companyRepository,
        IRepository<AccountingPeriod> periodRepository,
        IRepository<Journal> journalRepository,
        IRepository<Account> accountRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<BankTransaction> bankTransactionRepository,
        ILogger<MicroEntityAccountsGenerator> logger)
    {
        _companyRepository = companyRepository;
        _periodRepository = periodRepository;
        _journalRepository = journalRepository;
        _accountRepository = accountRepository;
        _invoiceRepository = invoiceRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _logger = logger;
    }

    public async Task<AccountsProductionResult> GenerateAsync(AccountsProductionRequest request, CancellationToken cancellationToken = default)
    {
        var result = new AccountsProductionResult { Success = true };

        try
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId, request.CompanyId, cancellationToken)
                ?? throw new InvalidOperationException("Company not found.");

            var period = await _periodRepository.GetByIdAsync(request.AccountingPeriodId, request.CompanyId, cancellationToken)
                ?? throw new InvalidOperationException("Accounting period not found.");

            var journals = await _journalRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);
            var accounts = await _accountRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);
            var invoices = await _invoiceRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);
            var transactions = await _bankTransactionRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);

            var periodJournals = journals
                .Where(j => j.AccountingPeriodId == request.AccountingPeriodId && j.Status == JournalStatus.Posted)
                .ToList();

            var periodInvoices = invoices
                .Where(i => i.Date >= request.PeriodStart && i.Date <= request.PeriodEnd)
                .ToList();

            var periodTransactions = transactions
                .Where(t => t.TransactionDate >= request.PeriodStart && t.TransactionDate <= request.PeriodEnd)
                .ToList();

            var ixbrlContent = GenerateMicroEntityIxbrl(company, period, periodJournals, accounts, periodInvoices, periodTransactions, request);

            result.IxbrlPath = $"micro_entity_accounts_{company.CompanyNumber}_{request.PeriodEnd:yyyyMMdd}.ixbrl";
            result.Success = true;
            result.GeneratedAt = DateTime.UtcNow;

            _logger.LogInformation("Micro-entity accounts generated for {CompanyNumber} period {Period}", company.CompanyNumber, request.PeriodEnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate micro-entity accounts for company {CompanyId}", request.CompanyId);
            result.Success = false;
            result.ErrorMessage = $"Failed to generate accounts: {ex.Message}";
        }

        return result;
    }

    private string GenerateMicroEntityIxbrl(Company company, AccountingPeriod period, List<Journal> journals, List<Account> accounts, List<Invoice> invoices, List<BankTransaction> transactions, AccountsProductionRequest request)
    {
        var totalRevenue = accounts
            .Where(a => a.Type == AccountType.Income)
            .Sum(a => journals.Where(j => j.Status == JournalStatus.Posted)
                .SelectMany(j => j.Lines)
                .Where(l => l.AccountId == a.Id)
                .Sum(l => l.Credit - l.Debit));

        var totalExpenses = accounts
            .Where(a => a.Type == AccountType.Expense)
            .Sum(a => journals.Where(j => j.Status == JournalStatus.Posted)
                .SelectMany(j => j.Lines)
                .Where(l => l.AccountId == a.Id)
                .Sum(l => l.Debit - l.Credit));

        var netProfit = totalRevenue - totalExpenses;

        return $"""
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml"
              xmlns:ix="http://www.xbrl.org/2013/inlineXBRL"
              xmlns:uk-gaap="http://www.xbrl.org/uk/gaap/core/2024-01-01">
        <head>
            <title>Micro-Entity Accounts - {company.CompanyName}</title>
        </head>
        <body>
            <h1>{company.CompanyName}</h1>
            <p>Company Number: {company.CompanyNumber}</p>
            <p>Micro-Entity Accounts for the year ended {request.PeriodEnd:dd MMMM yyyy}</p>

            <ix:nonNumeric name="uk-gaap:EntityLegalForm" contextRef="ctx1">Limited company</ix:nonNumeric>
            <ix:nonNumeric name="uk-gaap:NameOfEntity" contextRef="ctx1">{company.CompanyName}</ix:nonNumeric>
            <ix:nonNumeric name="uk-gaap:CompanyRegistrationNumber" contextRef="ctx1">{company.CompanyNumber}</ix:nonNumeric>

            <h2>Profit and Loss Account</h2>
            <ix:nonFraction name="uk-gaap:Turnover" contextRef="ctx1" unitRef="GBP" decimals="2">{totalRevenue}</ix:nonFraction>
            <ix:nonFraction name="uk-gaap:ProfitLoss" contextRef="ctx1" unitRef="GBP" decimals="2">{netProfit}</ix:nonFraction>

            <h2>Balance Sheet</h2>
            <ix:nonFraction name="uk-gaap:ShareCapital" contextRef="ctx1" unitRef="GBP" decimals="2">100</ix:nonFraction>

            <p>These accounts have been prepared in accordance with the micro-entity provisions of the Companies Act 2006.</p>
        </body>
        </html>
        """;
    }
}
