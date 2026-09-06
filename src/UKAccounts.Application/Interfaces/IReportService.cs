using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IReportService
{
    Task<TrialBalanceReport> GetTrialBalanceAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<ProfitLossReport> GetProfitLossAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<BalanceSheetReport> GetBalanceSheetAsync(Guid companyId, DateTime to, CancellationToken cancellationToken = default);
    Task<GeneralLedgerReport> GetGeneralLedgerAsync(Guid companyId, Guid accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<CashFlowReport> GetCashFlowAsync(Guid companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<AgedReceivablesReport> GetAgedReceivablesAsync(Guid companyId, DateTime asOfDate, CancellationToken cancellationToken = default);
    Task<AgedPayablesReport> GetAgedPayablesAsync(Guid companyId, DateTime asOfDate, CancellationToken cancellationToken = default);
}
