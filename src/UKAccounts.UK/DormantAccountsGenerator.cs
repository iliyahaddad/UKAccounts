using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.UK;

public class DormantAccountsGenerator : IAccountsGenerator
{
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<AccountingPeriod> _periodRepository;
    private readonly IRepository<Journal> _journalRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<BankTransaction> _bankTransactionRepository;
    private readonly ILogger<DormantAccountsGenerator> _logger;

    public DormantAccountsGenerator(
        IRepository<Company> companyRepository,
        IRepository<AccountingPeriod> periodRepository,
        IRepository<Journal> journalRepository,
        IRepository<Account> accountRepository,
        IRepository<BankTransaction> bankTransactionRepository,
        ILogger<DormantAccountsGenerator> logger)
    {
        _companyRepository = companyRepository;
        _periodRepository = periodRepository;
        _journalRepository = journalRepository;
        _accountRepository = accountRepository;
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

            if (!company.IsDormant)
            {
                result.Success = false;
                result.ErrorMessage = "Company is not marked as dormant.";
                return result;
            }

            var period = await _periodRepository.GetByIdAsync(request.AccountingPeriodId, request.CompanyId, cancellationToken)
                ?? throw new InvalidOperationException("Accounting period not found.");

            var journals = await _journalRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);
            var accounts = await _accountRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);
            var transactions = await _bankTransactionRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);

            var periodJournals = journals
                .Where(j => j.AccountingPeriodId == request.AccountingPeriodId && j.Status == JournalStatus.Posted)
                .ToList();

            var periodTransactions = transactions
                .Where(t => t.TransactionDate >= request.PeriodStart && t.TransactionDate <= request.PeriodEnd)
                .ToList();

            if (periodJournals.Any() || periodTransactions.Any())
            {
                result.Warnings.Add("Company is marked as dormant but has accounting activity in this period.");
            }

            var ixbrlContent = GenerateDormantIxbrl(company, period, periodJournals, accounts, periodTransactions, request.PeriodEnd);

            result.IxbrlPath = $"dormant_accounts_{company.CompanyNumber}_{request.PeriodEnd:yyyyMMdd}.ixbrl";
            result.Success = true;
            result.GeneratedAt = DateTime.UtcNow;

            _logger.LogInformation("Dormant accounts generated for {CompanyNumber} period {Period}", company.CompanyNumber, request.PeriodEnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate dormant accounts for company {CompanyId}", request.CompanyId);
            result.Success = false;
            result.ErrorMessage = $"Failed to generate accounts: {ex.Message}";
        }

        return result;
    }

    private string GenerateDormantIxbrl(Company company, AccountingPeriod period, List<Journal> journals, List<Account> accounts, List<BankTransaction> transactions, DateTime periodEnd)
    {
        var totalAssets = accounts
            .Where(a => a.Type == AccountType.Asset)
            .Sum(a => journals.Where(j => j.Status == JournalStatus.Posted)
                .SelectMany(j => j.Lines)
                .Where(l => l.AccountId == a.Id)
                .Sum(l => l.Debit - l.Credit));

        var totalLiabilities = accounts
            .Where(a => a.Type == AccountType.Liability)
            .Sum(a => journals.Where(j => j.Status == JournalStatus.Posted)
                .SelectMany(j => j.Lines)
                .Where(l => l.AccountId == a.Id)
                .Sum(l => l.Credit - l.Debit));

        var totalEquity = accounts
            .Where(a => a.Type == AccountType.Equity)
            .Sum(a => journals.Where(j => j.Status == JournalStatus.Posted)
                .SelectMany(j => j.Lines)
                .Where(l => l.AccountId == a.Id)
                .Sum(l => l.Credit - l.Debit));

        return $"""
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml"
              xmlns:ix="http://www.xbrl.org/2013/inlineXBRL"
              xmlns:uk-gaap="http://www.xbrl.org/uk/gaap/core/2024-01-01">
        <head>
            <title>Dormant Accounts - {company.CompanyName}</title>
        </head>
        <body>
            <h1>{company.CompanyName}</h1>
            <p>Company Number: {company.CompanyNumber}</p>
            <p>Dormant Accounts for the year ended {request.PeriodEnd:dd MMMM yyyy}</p>

            <ix:nonNumeric name="uk-gaap:EntityLegalForm" contextRef="ctx1">Limited company</ix:nonNumeric>
            <ix:nonNumeric name="uk-gaap:NameOfEntity" contextRef="ctx1">{company.CompanyName}</ix:nonNumeric>
            <ix:nonNumeric name="uk-gaap:CompanyRegistrationNumber" contextRef="ctx1">{company.CompanyNumber}</ix:nonNumeric>

            <h2>Balance Sheet</h2>
            <ix:nonFraction name="uk-gaap:ShareCapital" contextRef="ctx1" unitRef="GBP" decimals="2">{totalEquity}</ix:nonFraction>
            <ix:nonFraction name="uk-gaap:TotalAssets" contextRef="ctx1" unitRef="GBP" decimals="2">{totalAssets}</ix:nonFraction>
            <ix:nonFraction name="uk-gaap:TotalLiabilities" contextRef="ctx1" unitRef="GBP" decimals="2">{totalLiabilities}</ix:nonFraction>

            <p>These accounts have been prepared as dormant accounts in accordance with the Companies Act 2006.</p>
        </body>
        </html>
        """;
    }
}
