using System.Diagnostics;
using System.Text.Json;

namespace UKAccounts.Xbrl;

public class IxbrlReporterAdapter : IIxbrlGenerator
{
    private readonly string _pythonPath;
    private readonly string _ixbrlReporterPath;

    public IxbrlReporterAdapter(string pythonPath, string ixbrlReporterPath)
    {
        _pythonPath = pythonPath;
        _ixbrlReporterPath = ixbrlReporterPath;
    }

    public async Task<GenerationResult> GenerateAsync(IxbrlRequest request, CancellationToken cancellationToken = default)
    {
        var result = new GenerationResult();

        try
        {
            var inputJson = JsonSerializer.Serialize(new
            {
                company_id = request.CompanyId.ToString(),
                company_number = request.CompanyNumber,
                company_name = request.CompanyName,
                period_start = request.PeriodStart.ToString("yyyy-MM-dd"),
                period_end = request.PeriodEnd.ToString("yyyy-MM-dd"),
                regime = request.Regime.ToString()
            });

            var tempInput = Path.Combine(Path.GetTempPath(), $"ixbrl_input_{Guid.NewGuid()}.json");
            var tempOutput = Path.Combine(Path.GetTempPath(), $"ixbrl_output_{Guid.NewGuid()}.ixbrl");

            await File.WriteAllTextAsync(tempInput, inputJson, cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonPath,
                ArgumentList =
                {
                    Path.Combine(_ixbrlReporterPath, "generate.py"),
                    "--input", tempInput,
                    "--output", tempOutput,
                    "--format", "ixbrl"
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                result.Success = false;
                result.ErrorMessage = "Failed to start ixbrl-reporter process.";
                return result;
            }

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0 && File.Exists(tempOutput))
            {
                result.Success = true;
                result.OutputPath = tempOutput;
                result.IxbrlContent = await File.ReadAllTextAsync(tempOutput, cancellationToken);
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = $"ixbrl-reporter failed with exit code {process.ExitCode}.";
            }

            File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Failed to generate iXBRL: {ex.Message}";
        }

        return result;
    }
}
