using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<InvoiceLine> _lineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IRepository<Invoice> invoiceRepository,
        IRepository<InvoiceLine> lineRepository,
        IUnitOfWork unitOfWork,
        ILogger<InvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _lineRepository = lineRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = new Invoice
        {
            CompanyId = request.CompanyId,
            CustomerId = request.CustomerId,
            InvoiceNumber = request.InvoiceNumber,
            Date = request.Date,
            DueDate = request.DueDate,
            Currency = request.Currency,
            Status = InvoiceStatus.Draft,
            Notes = request.Notes
        };

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var line in request.Lines)
        {
            var invoiceLine = new InvoiceLine
            {
                InvoiceId = invoice.Id,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TaxCode = line.TaxCode,
                TaxAmount = line.TaxAmount,
                LineTotal = line.LineTotal,
                SortOrder = line.SortOrder
            };

            await _lineRepository.AddAsync(invoiceLine, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice created {InvoiceId} {InvoiceNumber}", invoice.Id, invoice.InvoiceNumber);
        return await ToDtoAsync(invoice);
    }

    public async Task<InvoiceDto?> GetAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, Guid.Empty, cancellationToken);
        return invoice == null ? null : await ToDtoAsync(invoice);
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetByCompanyAsync(companyId, cancellationToken);
        var result = new List<InvoiceDto>();

        foreach (var invoice in invoices)
        {
            result.Add(await ToDtoAsync(invoice));
        }

        return result;
    }

    public async Task<InvoiceDto> UpdateAsync(Guid invoiceId, CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (invoice.Status == InvoiceStatus.Issued || invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Cannot update a issued or paid invoice.");
        }

        invoice.CustomerId = request.CustomerId;
        invoice.InvoiceNumber = request.InvoiceNumber;
        invoice.Date = request.Date;
        invoice.DueDate = request.DueDate;
        invoice.Currency = request.Currency;
        invoice.Notes = request.Notes;

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice updated {InvoiceId}", invoice.Id);
        return await ToDtoAsync(invoice);
    }

    public async Task DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (invoice.Status == InvoiceStatus.Issued || invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Cannot delete an issued or paid invoice.");
        }

        await _invoiceRepository.DeleteAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice deleted {InvoiceId}", invoiceId);
    }

    public async Task<InvoiceDto> IssueAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("Only draft invoices can be issued.");
        }

        invoice.Status = InvoiceStatus.Issued;
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice issued {InvoiceId}", invoiceId);
        return await ToDtoAsync(invoice);
    }

    public async Task<InvoiceDto> MarkAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (invoice.Status != InvoiceStatus.Issued && invoice.Status != InvoiceStatus.PartiallyPaid)
        {
            throw new InvalidOperationException("Only issued or partially paid invoices can be marked as paid.");
        }

        invoice.Status = InvoiceStatus.Paid;
        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice marked as paid {InvoiceId}", invoiceId);
        return await ToDtoAsync(invoice);
    }

    private async Task<InvoiceDto> ToDtoAsync(Invoice invoice)
    {
        var lines = await _lineRepository.GetByCompanyAsync(invoice.CompanyId, cancellationToken: default);
        var invoiceLines = lines.Where(l => l.InvoiceId == invoice.Id).ToList();

        return new InvoiceDto
        {
            Id = invoice.Id,
            CompanyId = invoice.CompanyId,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.Name ?? string.Empty,
            InvoiceNumber = invoice.InvoiceNumber,
            Date = invoice.Date,
            DueDate = invoice.DueDate,
            Currency = invoice.Currency,
            Status = invoice.Status,
            SubTotal = invoice.SubTotal,
            TaxTotal = invoice.TaxTotal,
            Total = invoice.Total,
            Notes = invoice.Notes,
            Lines = invoiceLines.Select(l => new InvoiceLineDto
            {
                Id = l.Id,
                InvoiceId = l.InvoiceId,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxCode = l.TaxCode,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                SortOrder = l.SortOrder
            }).ToList()
        };
    }
}
