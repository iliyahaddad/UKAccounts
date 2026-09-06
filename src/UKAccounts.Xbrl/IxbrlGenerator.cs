namespace UKAccounts.Xbrl;

public class IxbrlGenerator : IIxbrlGenerator
{
    private readonly NativeIxbrlGenerator _nativeGenerator;

    public IxbrlGenerator()
    {
        _nativeGenerator = new NativeIxbrlGenerator();
    }

    public Task<GenerationResult> GenerateAsync(IxbrlRequest request, CancellationToken cancellationToken = default)
    {
        return _nativeGenerator.GenerateAsync(request, cancellationToken);
    }
}
