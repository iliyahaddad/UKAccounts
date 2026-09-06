namespace UKAccounts.Application.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountCategory Category { get; set; }
    public AccountType Type { get; set; }
    public string? ParentCode { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
