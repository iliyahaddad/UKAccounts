using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class BillService : IBillService
{
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<BillLine> _lineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillService> _logger;

    public BillService(
        IRepository<Bill> billRepository,
        IRepository<BillLine> lineRepository,
        IUnitOfWork unitOfWork,
        ILogger<BillService> logger)
    {
        _billRepository = billRepository;
        _lineRepository = lineRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BillDto> CreateAsync(CreateBillRequest request, CancellationToken cancellationToken = default)
    {
        var bill = new Bill
        {
            CompanyId = request.CompanyId,
            SupplierId = request.SupplierId,
            BillNumber = request.BillNumber,
            Date = request.Date,
            DueDate = request.DueDate,
            Currency = request.Currency,
            Status = BillStatus.Draft,
            Notes = request.Notes
        };

        await _billRepository.AddAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var line in request.Lines)
        {
            var billLine = new BillLine
            {
                BillId = bill.Id,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TaxCode = line.TaxCode,
                TaxAmount = line.TaxAmount,
                LineTotal = line.LineTotal,
                SortOrder = line.SortOrder
            };

            await _lineRepository.AddAsync(billLine, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bill created {BillId} {BillNumber}", bill.Id, bill.BillNumber);
        return await ToDtoAsync(bill);
    }

    public async Task<BillDto?> GetAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(billId, Guid.Empty, cancellationToken);
        return bill == null ? null : await ToDtoAsync(bill);
    }

    public async Task<IReadOnlyList<BillDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var bills = await _billRepository.GetByCompanyAsync(companyId, cancellationToken);
        var result = new List<BillDto>();

        foreach (var bill in bills)
        {
            result.Add(await ToDtoAsync(bill));
        }

        return result;
    }

    public async Task<BillDto> UpdateAsync(Guid billId, CreateBillRequest request, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(billId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Bill not found.");

        if (bill.Status == BillStatus.Approved || bill.Status == BillStatus.Paid)
        {
            throw new InvalidOperationException("Cannot update an approved or paid bill.");
        }

        bill.SupplierId = request.SupplierId;
        bill.BillNumber = request.BillNumber;
        bill.Date = request.Date;
        bill.DueDate = request.DueDate;
        bill.Currency = request.Currency;
        bill.Notes = request.Notes;

        await _billRepository.UpdateAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bill updated {BillId}", bill.Id);
        return await ToDtoAsync(bill);
    }

    public async Task DeleteAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(billId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Bill not found.");

        if (bill.Status == BillStatus.Approved || bill.Status == BillStatus.Paid)
        {
            throw new InvalidOperationException("Cannot delete an approved or paid bill.");
        }

        await _billRepository.DeleteAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bill deleted {BillId}", billId);
    }

    public async Task<BillDto> ApproveAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(billId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Bill not found.");

        if (bill.Status != BillStatus.Draft)
        {
            throw new InvalidOperationException("Only draft bills can be approved.");
        }

        bill.Status = BillStatus.Approved;
        await _billRepository.UpdateAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bill approved {BillId}", billId);
        return await ToDtoAsync(bill);
    }

    public async Task<BillDto> MarkAsPaidAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var bill = await _billRepository.GetByIdAsync(billId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Bill not found.");

        if (bill.Status != BillStatus.Approved && bill.Status != BillStatus.PartiallyPaid)
        {
            throw new InvalidOperationException("Only approved or partially paid bills can be marked as paid.");
        }

        bill.Status = BillStatus.Paid;
        await _billRepository.UpdateAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bill marked as paid {BillId}", billId);
        return await ToDtoAsync(bill);
    }

    private async Task<BillDto> ToDtoAsync(Bill bill)
    {
        var lines = await _lineRepository.GetByCompanyAsync(bill.CompanyId, cancellationToken: default);
        var billLines = lines.Where(l => l.BillId == bill.Id).ToList();

        return new BillDto
        {
            Id = bill.Id,
            CompanyId = bill.CompanyId,
            SupplierId = bill.SupplierId,
            SupplierName = bill.Supplier?.Name ?? string.Empty,
            BillNumber = bill.BillNumber,
            Date = bill.Date,
            DueDate = bill.DueDate,
            Currency = bill.Currency,
            Status = bill.Status,
            SubTotal = bill.SubTotal,
            TaxTotal = bill.TaxTotal,
            Total = bill.Total,
            Notes = bill.Notes,
            Lines = billLines.Select(l => new BillLineDto
            {
                Id = l.Id,
                BillId = l.BillId,
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
