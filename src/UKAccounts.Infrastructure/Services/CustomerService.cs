using Microsoft.Extensions.Logging;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using UKAccounts.Domain.Entities;
using UKAccounts.Domain.Interfaces;

namespace UKAccounts.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            CompanyId = request.CompanyId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            ContactPerson = request.ContactPerson
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer created {CustomerId} {Name}", customer.Id, customer.Name);
        return ToDto(customer);
    }

    public async Task<CustomerDto?> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, Guid.Empty, cancellationToken);
        return customer == null ? null : ToDto(customer);
    }

    public async Task<IReadOnlyList<CustomerDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetByCompanyAsync(companyId, cancellationToken);
        return customers.Select(ToDto).ToList();
    }

    public async Task<CustomerDto> UpdateAsync(Guid customerId, CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Address = request.Address;
        customer.ContactPerson = request.ContactPerson;

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer updated {CustomerId}", customer.Id);
        return ToDto(customer);
    }

    public async Task DeleteAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, Guid.Empty, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        await _customerRepository.DeleteAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer deleted {CustomerId}", customerId);
    }

    private static CustomerDto ToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            ContactPerson = customer.ContactPerson,
            IsActive = customer.IsActive,
            InvoiceCount = customer.Invoices?.Count ?? 0
        };
    }
}
