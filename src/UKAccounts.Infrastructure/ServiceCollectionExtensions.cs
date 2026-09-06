using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.Interfaces;
using UKAccounts.Infrastructure.Services;

namespace UKAccounts.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=ukaccounts.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            }));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, AppDbContext>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ICompaniesHouseLookupService, CompaniesHouseLookupService>();
        services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
        services.AddScoped<IJournalService, JournalService>();
        services.AddScoped<IAccountingEngine, AccountingEngine>();
        services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
        services.AddScoped<ITrialBalanceService, TrialBalanceService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IBillService, BillService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IBankTransactionService, BankTransactionService>();
        services.AddScoped<IBankStatementService, BankStatementService>();
        services.AddScoped<IImportMappingService, ImportMappingService>();
        services.AddScoped<BankImportService>();
        services.AddScoped<BankReconciliationService>();
        services.AddScoped<IArelleValidator, ArelleValidator>();
        services.AddScoped<IFilingService, FilingService>();

        return services;
    }
}
