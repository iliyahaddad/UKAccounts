using UKAccounts.Application.DTOs;

namespace UKAccounts.Application.Interfaces;

public interface IArelleValidator
{
    Task<ArelleValidationResult> ValidateAsync(string ixbrlPath, ValidationOptions? options = null, CancellationToken cancellationToken = default);
}

public class ValidationOptions
{
    public bool CheckCalculations { get; set; } = true;
    public bool CheckDimensions { get; set; } = true;
    public bool CheckUnits { get; set; } = true;
    public bool HmrcRules { get; set; } = true;
    public string TaxonomyId { get; set; } = "UK";
}
