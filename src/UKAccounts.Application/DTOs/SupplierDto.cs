namespace UKAccounts.Application.DTOs;

public class SupplierDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsActive { get; set; }
    public int BillCount { get; set; }
    public int ExpenseCount { get; set; }
}
