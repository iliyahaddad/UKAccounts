using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IRepository<BankAccount> _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BankAccountService> _logger;

    public BankAccountService(
        IRepository<BankAccount> bankAccountRepository,
        IUnitOfWork unitOfWork,
        ILogger<BankAccountService> logger)
    {
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BankAccountDto> CreateAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var bankAccount = new BankAccount
        {
            CompanyId = request.CompanyId,
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber,
            SortCode = request.SortCode,
            Currency = request.Currency,
            OpeningBalance = request.OpeningBalance
        };

        await _bankAccountRepository.AddAsync(bankAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank account created {BankAccountId}", bankAccount.Id);
        return ToDto(bankAccount);
    }

    public async Task<BankAccountDto?> GetAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, Guid.Empty, cancellationToken);
        return bankAccount == null ? null : ToDto(bankAccount);
    }

    public async Task<IReadOnlyList<BankAccountDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var bankAccounts = await _bankAccountRepository.GetByCompanyAsync(companyId, cancellationToken);
        return bankAccounts.Select(ToDto).ToList();
    }

    public async Task<BankAccountDto> UpdateAsync(Guid bankAccountId, CreateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Bank account not found.");

        bankAccount.AccountName = request.AccountName;
        bankAccount.AccountNumber = request.AccountNumber;
        bankAccount.SortCode = request.SortCode;
        bankAccount.Currency = request.Currency;
        bankAccount.OpeningBalance = request.OpeningBalance;

        await _bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank account updated {BankAccountId}", bankAccount.Id);
        return ToDto(bankAccount);
    }

    public async Task DeleteAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Bank account not found.");

        await _bankAccountRepository.DeleteAsync(bankAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bank account deleted {BankAccountId}", bankAccountId);
    }

    private static BankAccountDto ToDto(BankAccount bankAccount)
    {
        return new BankAccountDto
        {
            Id = bankAccount.Id,
            CompanyId = bankAccount.CompanyId,
            AccountName = bankAccount.AccountName,
            AccountNumber = bankAccount.AccountNumber,
            SortCode = bankAccount.SortCode,
            Currency = bankAccount.Currency,
            OpeningBalance = bankAccount.OpeningBalance,
            IsActive = bankAccount.IsActive
        };
    }
}
