using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;
using UKAccounts.Infrastructure.Persistence;
using UKAccounts.Infrastructure.Services;
using Xunit;

namespace UKAccounts.Tests;

public class InvoiceServiceTests
{
    private async Task<(AppDbContext context, CustomerService customerService, InvoiceService invoiceService)> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var customerRepository = new Repository<Customer>(context);
        var invoiceRepository = new Repository<Invoice>(context);
        var lineRepository = new Repository<InvoiceLine>(context);

        var logger = NullLogger<InvoiceService>.Instance;

        var customerService = new CustomerService(customerRepository, context, logger);
        var invoiceService = new InvoiceService(invoiceRepository, lineRepository, context, logger);

        return (context, customerService, invoiceService);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateInvoice()
    {
        using var (context, customerService, invoiceService) = await CreateContextAsync();

        var customer = await customerService.CreateAsync(new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer"
        });

        var request = new CreateInvoiceRequest
        {
            CompanyId = Guid.Empty,
            CustomerId = customer.Id,
            InvoiceNumber = "INV-001",
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Lines = new List<CreateInvoiceLineRequest>
            {
                new CreateInvoiceLineRequest
                {
                    Description = "Test Item",
                    Quantity = 1,
                    UnitPrice = 100,
                    LineTotal = 100
                }
            }
        };

        var invoice = await invoiceService.CreateAsync(request);

        Assert.NotNull(invoice);
        Assert.Equal("INV-001", invoice.InvoiceNumber);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }

    [Fact]
    public async Task IssueAsync_ShouldChangeStatusToIssued()
    {
        using var (context, customerService, invoiceService) = await CreateContextAsync();

        var customer = await customerService.CreateAsync(new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer"
        });

        var request = new CreateInvoiceRequest
        {
            CompanyId = Guid.Empty,
            CustomerId = customer.Id,
            InvoiceNumber = "INV-002",
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Lines = new List<CreateInvoiceLineRequest>
            {
                new CreateInvoiceLineRequest
                {
                    Description = "Test Item",
                    Quantity = 1,
                    UnitPrice = 100,
                    LineTotal = 100
                }
            }
        };

        var invoice = await invoiceService.CreateAsync(request);
        var issued = await invoiceService.IssueAsync(invoice.Id);

        Assert.Equal(InvoiceStatus.Issued, issued.Status);
    }

    [Fact]
    public async Task MarkAsPaidAsync_ShouldChangeStatusToPaid()
    {
        using var (context, customerService, invoiceService) = await CreateContextAsync();

        var customer = await customerService.CreateAsync(new CreateCustomerRequest
        {
            CompanyId = Guid.Empty,
            Name = "Test Customer"
        });

        var request = new CreateInvoiceRequest
        {
            CompanyId = Guid.Empty,
            CustomerId = customer.Id,
            InvoiceNumber = "INV-003",
            Date = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Lines = new List<CreateInvoiceLineRequest>
            {
                new CreateInvoiceLineRequest
                {
                    Description = "Test Item",
                    Quantity = 1,
                    UnitPrice = 100,
                    LineTotal = 100
                }
            }
        };

        var invoice = await invoiceService.CreateAsync(request);
        await invoiceService.IssueAsync(invoice.Id);
        var paid = await invoiceService.MarkAsPaidAsync(invoice.Id);

        Assert.Equal(InvoiceStatus.Paid, paid.Status);
    }
}
