using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChartOfAccountsService> _logger;

    public ChartOfAccountsService(
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<ChartOfAccountsService> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            CompanyId = request.CompanyId,
            Code = request.Code,
            Name = request.Name,
            Category = request.Category,
            Type = request.Type,
            ParentCode = request.ParentCode,
            SortOrder = request.SortOrder
        };

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account created {AccountId} {Code}", account.Id, account.Code);
        return ToDto(account);
    }

    public async Task<AccountDto?> GetAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, Guid.Empty, cancellationToken);
        return account == null ? null : ToDto(account);
    }

    public async Task<IReadOnlyList<AccountDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);
        return accounts.Select(ToDto).OrderBy(a => a.SortOrder).ThenBy(a => a.Code).ToList();
    }

    public async Task<AccountDto> UpdateAsync(Guid accountId, CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Account not found.");

        account.Code = request.Code;
        account.Name = request.Name;
        account.Category = request.Category;
        account.Type = request.Type;
        account.ParentCode = request.ParentCode;
        account.SortOrder = request.SortOrder;

        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account updated {AccountId}", account.Id);
        return ToDto(account);
    }

    public async Task DeleteAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Account not found.");

        await _accountRepository.DeleteAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account deleted {AccountId}", accountId);
    }

    public async Task ReorderAsync(Guid companyId, List<Guid> orderedAccountIds, CancellationToken cancellationToken = default)
    {
        var accounts = await _accountRepository.GetByCompanyAsync(companyId, cancellationToken);
        var accountDict = accounts.ToDictionary(a => a.Id);

        for (int i = 0; i < orderedAccountIds.Count; i++)
        {
            if (accountDict.TryGetValue(orderedAccountIds[i], out var account))
            {
                account.SortOrder = i;
                await _accountRepository.UpdateAsync(account, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AccountDto ToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            CompanyId = account.CompanyId,
            Code = account.Code,
            Name = account.Name,
            Category = account.Category,
            Type = account.Type,
            ParentCode = account.ParentCode,
            IsActive = account.IsActive,
            SortOrder = account.SortOrder
        };
    }
}
