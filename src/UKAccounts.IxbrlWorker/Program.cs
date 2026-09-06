using System.CommandLine;
using System.Text.Json;
using UKAccounts.Xbrl;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var inputOption = new Option<string>("--input");
        var outputOption = new Option<string>("--output");

        var rootCommand = new RootCommand("iXBRL Generation Worker")
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

                var json = await File.ReadAllTextAsync(input);
                var request = JsonSerializer.Deserialize<IxbrlRequest>(json);

                if (request == null)
                {
                    Console.Error.WriteLine("Invalid input JSON.");
                    Environment.Exit(1);
                }

                var generator = new NativeIxbrlGenerator();
                var result = await generator.GenerateAsync(request);

                if (!result.Success)
                {
                    Console.Error.WriteLine($"Generation failed: {result.ErrorMessage}");
                    Environment.Exit(1);
                }

                if (!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(result.IxbrlContent))
                {
                    await File.WriteAllTextAsync(output, result.IxbrlContent);
                }

                Console.WriteLine(JsonSerializer.Serialize(new
                {
                    success = true,
                    outputPath = result.OutputPath,
                    generatedAt = DateTime.UtcNow
                }));
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
