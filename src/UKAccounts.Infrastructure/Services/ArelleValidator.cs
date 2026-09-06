using System.Text.Json;
using UKAccounts.Application.DTOs;
using UKAccounts.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace UKAccounts.Infrastructure.Services;

public class ArelleValidator : IArelleValidator
{
    private readonly string _arelleWorkerPath;
    private readonly ILogger<ArelleValidator> _logger;

    public ArelleValidator(ILogger<ArelleValidator> logger)
    {
        _logger = logger;
        _arelleWorkerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UKAccounts.ArelleWorker.exe");
    }

    public async Task<ArelleValidationResult> ValidateAsync(string ixbrlPath, ValidationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = new ArelleValidationResult();
        options ??= new ValidationOptions();

        try
        {
            if (!File.Exists(ixbrlPath))
            {
                result.Errors.Add(new ValidationMessage
                {
                    Code = "FILE_NOT_FOUND",
                    Message = $"iXBRL file not found: {ixbrlPath}",
                    Severity = "Error"
                });
                return result;
            }

            var inputJson = JsonSerializer.Serialize(new
            {
                input = ixbrlPath,
                taxonomy = options.TaxonomyId,
                options = new
                {
                    checkCalculations = options.CheckCalculations,
                    checkDimensions = options.CheckDimensions,
                    checkUnits = options.CheckUnits,
                    hmrcRules = options.HmrcRules
                }
            });

            var tempInput = Path.Combine(Path.GetTempPath(), $"arelle_input_{Guid.NewGuid()}.json");
            var tempOutput = Path.Combine(Path.GetTempPath(), $"arelle_output_{Guid.NewGuid()}.json");

            await File.WriteAllTextAsync(tempInput, inputJson, cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = _arelleWorkerPath,
                ArgumentList = { "--input", tempInput, "--output", tempOutput },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                result.Errors.Add(new ValidationMessage
                {
                    Code = "PROCESS_START_FAILED",
                    Message = "Failed to start Arelle worker process.",
                    Severity = "Error"
                });
                return result;
            }

            await process.WaitForExitAsync(cancellationToken);
            sw.Stop();

            result.Duration = sw.Elapsed;

            if (process.ExitCode != 0)
            {
                var errorOutput = await File.ReadAllTextAsync(tempOutput, cancellationToken);
                result.Errors.Add(new ValidationMessage
                {
                    Code = "ARELLE_FAILED",
                    Message = $"Arelle validation failed with exit code {process.ExitCode}. {errorOutput}",
                    Severity = "Error"
                });
                return result;
            }

            if (File.Exists(tempOutput))
            {
                var outputJson = await File.ReadAllTextAsync(tempOutput, cancellationToken);
                var arelleResult = JsonSerializer.Deserialize<ArelleWorkerResult>(outputJson);
                
                if (arelleResult != null)
                {
                    result.IsValid = arelleResult.IsValid;
                    result.ArelleVersion = arelleResult.ArelleVersion ?? "unknown";

                    foreach (var error in arelleResult.Errors ?? new())
                    {
                        result.Errors.Add(new ValidationMessage
                        {
                            Code = error.Code,
                            Message = error.Message,
                            Severity = "Error",
                            FactReference = error.FactReference,
                            LineNumber = error.LineNumber
                        });
                    }

                    foreach (var warning in arelleResult.Warnings ?? new())
                    {
                        result.Warnings.Add(new ValidationMessage
                        {
                            Code = warning.Code,
                            Message = warning.Message,
                            Severity = "Warning",
                            FactReference = warning.FactReference,
                            LineNumber = warning.LineNumber
                        });
                    }

                    foreach (var info in arelleResult.Infos ?? new())
                    {
                        result.Infos.Add(new ValidationMessage
                        {
                            Code = info.Code,
                            Message = info.Message,
                            Severity = "Info",
                            FactReference = info.FactReference,
                            LineNumber = info.LineNumber
                        });
                    }
                }
            }

            _logger.LogInformation("Arelle validation completed in {Duration}ms. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", 
                result.Duration.TotalMilliseconds, result.IsValid, result.Errors.Count, result.Warnings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arelle validation failed");
            result.Errors.Add(new ValidationMessage
            {
                Code = "VALIDATION_EXCEPTION",
                Message = $"Validation failed: {ex.Message}",
                Severity = "Error"
            });
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }

        return result;
    }

    private class ArelleWorkerResult
    {
        public bool IsValid { get; set; }
        public string? ArelleVersion { get; set; }
        public List<ValidationMessage>? Errors { get; set; }
        public List<ValidationMessage>? Warnings { get; set; }
        public List<ValidationMessage>? Infos { get; set; }
    }
}
