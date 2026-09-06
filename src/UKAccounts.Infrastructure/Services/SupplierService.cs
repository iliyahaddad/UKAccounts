using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        IRepository<Supplier> supplierRepository,
        IUnitOfWork unitOfWork,
        ILogger<SupplierService> logger)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier
        {
            CompanyId = request.CompanyId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            ContactPerson = request.ContactPerson
        };

        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier created {SupplierId} {Name}", supplier.Id, supplier.Name);
        return ToDto(supplier);
    }

    public async Task<SupplierDto?> GetAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId, Guid.Empty, cancellationToken);
        return supplier == null ? null : ToDto(supplier);
    }

    public async Task<IReadOnlyList<SupplierDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var suppliers = await _supplierRepository.GetByCompanyAsync(companyId, cancellationToken);
        return suppliers.Select(ToDto).ToList();
    }

    public async Task<SupplierDto> UpdateAsync(Guid supplierId, CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Supplier not found.");

        supplier.Name = request.Name;
        supplier.Email = request.Email;
        supplier.Phone = request.Phone;
        supplier.Address = request.Address;
        supplier.ContactPerson = request.ContactPerson;

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier updated {SupplierId}", supplier.Id);
        return ToDto(supplier);
    }

    public async Task DeleteAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(supplierId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Supplier not found.");

        await _supplierRepository.DeleteAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Supplier deleted {SupplierId}", supplierId);
    }

    private static SupplierDto ToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            CompanyId = supplier.CompanyId,
            Name = supplier.Name,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            ContactPerson = supplier.ContactPerson,
            IsActive = supplier.IsActive,
            BillCount = supplier.Bills?.Count ?? 0,
            ExpenseCount = supplier.Expenses?.Count ?? 0
        };
    }
}
