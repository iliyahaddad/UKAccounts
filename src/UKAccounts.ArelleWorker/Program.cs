using System.CommandLine;
using System.Text.Json;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var inputOption = new Option<string>("--input");
        var outputOption = new Option<string>("--output");

        var rootCommand = new RootCommand("Arelle Validation Worker")
        {
            inputOption,
            outputOption
        };

        rootCommand.SetHandler(async (input, output) =>
        {
            try
            {
                if (!File.Exists(input))
                {
                    Console.Error.WriteLine($"Input file not found: {input}");
                    Environment.Exit(1);
                }

                var validationResult = new
                {
                    isValid = true,
                    errors = new object[] { },
                    warnings = new object[] { },
                    infos = new object[] { },
                    durationMs = 0,
                    arelleVersion = "stub"
                };

                if (!string.IsNullOrEmpty(output))
                {
                    await File.WriteAllTextAsync(output, JsonSerializer.Serialize(validationResult));
                }

                Console.WriteLine(JsonSerializer.Serialize(validationResult));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, inputOption, outputOption);

        return await rootCommand.InvokeAsync(args);
    }
}
