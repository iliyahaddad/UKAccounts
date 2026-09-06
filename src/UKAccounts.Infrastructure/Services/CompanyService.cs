using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class CompanyService : ICompanyService
{
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<AccountingPeriod> _accountingPeriodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        IRepository<Company> companyRepository,
        IRepository<AccountingPeriod> accountingPeriodRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompanyService> logger)
    {
        _companyRepository = companyRepository;
        _accountingPeriodRepository = accountingPeriodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default)
    {
        var company = new Company
        {
            CompanyNumber = request.CompanyNumber,
            CompanyName = request.CompanyName,
            RegisteredOffice = request.RegisteredOffice,
            CompanyType = request.CompanyType,
            IncorporationDate = request.IncorporationDate,
            SicCode = request.SicCode,
            AccountingReferenceDate = request.AccountingReferenceDate,
            Currency = request.Currency,
            Regime = request.Regime,
            IsDormant = request.Regime == AccountsRegime.Dormant
        };

        await _companyRepository.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company created {CompanyId} {CompanyNumber}", company.Id, company.CompanyNumber);

        return ToDto(company);
    }

    public async Task<CompanyDto?> GetAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(companyId, Guid.Empty, cancellationToken);
        return company == null ? null : ToDto(company);
    }

    public async Task<IReadOnlyList<CompanyDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var companies = await _companyRepository.GetByCompanyAsync(Guid.Empty, cancellationToken);
        return companies.Select(ToDto).ToList();
    }

    public async Task<CompanyDto> UpdateAsync(Guid companyId, CreateCompanyRequest request, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(companyId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Company not found.");

        company.CompanyNumber = request.CompanyNumber;
        company.CompanyName = request.CompanyName;
        company.RegisteredOffice = request.RegisteredOffice;
        company.CompanyType = request.CompanyType;
        company.IncorporationDate = request.IncorporationDate;
        company.SicCode = request.SicCode;
        company.AccountingReferenceDate = request.AccountingReferenceDate;
        company.Currency = request.Currency;
        company.Regime = request.Regime;
        company.IsDormant = request.Regime == AccountsRegime.Dormant;

        await _companyRepository.UpdateAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company updated {CompanyId}", company.Id);
        return ToDto(company);
    }

    public async Task DeleteAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(companyId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Company not found.");

        await _companyRepository.DeleteAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company deleted {CompanyId}", companyId);
    }

    public async Task<AccountingPeriodDto> CreateAccountingPeriodAsync(Guid companyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var period = new AccountingPeriod
        {
            CompanyId = companyId,
            StartDate = startDate,
            EndDate = endDate,
            IsCurrent = false
        };

        await _accountingPeriodRepository.AddAsync(period, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AccountingPeriodDto
        {
            Id = period.Id,
            CompanyId = period.CompanyId,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            IsCurrent = period.IsCurrent,
            FiledAt = period.FiledAt
        };
    }

    public async Task<IReadOnlyList<AccountingPeriodDto>> GetAccountingPeriodsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var periods = await _accountingPeriodRepository.GetByCompanyAsync(companyId, cancellationToken);
        return periods.Select(p => new AccountingPeriodDto
        {
            Id = p.Id,
            CompanyId = p.CompanyId,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsCurrent = p.IsCurrent,
            FiledAt = p.FiledAt
        }).ToList();
    }

    private static CompanyDto ToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            CompanyNumber = company.CompanyNumber,
            CompanyName = company.CompanyName,
            RegisteredOffice = company.RegisteredOffice,
            CompanyType = company.CompanyType,
            IncorporationDate = company.IncorporationDate,
            SicCode = company.SicCode,
            AccountingReferenceDate = company.AccountingReferenceDate,
            Currency = company.Currency,
            Regime = company.Regime,
            IsDormant = company.IsDormant,
            AccountingPeriodCount = company.AccountingPeriods?.Count ?? 0
        };
    }
}
