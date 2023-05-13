using JsonDiffPatchDotNet;

using Newtonsoft.Json.Linq;

namespace GitOpsConfig.TestHarness.Cli.Bases;

public abstract class BaseGenerateCommand : BaseCommand
{
    private string? _outputDir;
    private string? _referenceDir;
    private string? _comparisonDir;

    public sealed override async ValueTask HandleCommandAsync(IParseResult parseResult)
    {
        string rootDir = RootDir.FullName;
        _outputDir = Path.Combine(rootDir, "_processing", "output");
        _referenceDir = Path.Combine(rootDir, "_processing", "references");
        _comparisonDir = Path.Combine(rootDir, "_processing", "comparisons");

        if (!Directory.Exists(_outputDir))
            Directory.CreateDirectory(_outputDir);
        if (!Directory.Exists(_referenceDir))
            Directory.CreateDirectory(_referenceDir);
        if (!Directory.Exists(_comparisonDir))
            Directory.CreateDirectory(_comparisonDir);

        await GenerateAsync(rootDir);
    }

    protected abstract ValueTask GenerateAsync(string rootDir);

    protected string OutputDir => _outputDir ?? throw new InvalidOperationException("Output directory not initialized.");

    protected string ReferenceDir => _referenceDir ?? throw new InvalidOperationException("Reference directory not initialized.");

    protected string ComparisonDir => _comparisonDir ?? throw new InvalidOperationException("Comparison directory not initialized.");

    protected static string? CompareConfigs(string outputDir, string referenceDir, string fileName)
    {
        string referenceFilePath = Path.Combine(referenceDir, fileName);
        if (!File.Exists(referenceFilePath))
            return null;

        string outputFilePath = Path.Combine(outputDir, fileName);

        JObject reference = JObject.Parse(File.ReadAllText(referenceFilePath));
        JObject output = JObject.Parse(File.ReadAllText(outputFilePath));

        JsonDiffPatch differ = new();
        JToken diff = differ.Diff(reference, output);
        return diff?.ToString();
    }

    protected async Task HandleConfig(GeneratedConfiguration config, string app, string[] sections)
    {
        string displaySection = string.Join('/', sections);
        string displayFileName =
            $"[{Green1}]{app}[/] | [{Magenta1}]{displaySection}[/] | [{Yellow1}]{config.FileName}[/]";
        MarkupLine(displayFileName);

        string sectionStr = string.Join('.', sections);
        string fileName = $"{app}_{sectionStr}_{config.FileName}";
        string filePath = Path.Combine(OutputDir, fileName);
        await File.WriteAllTextAsync(filePath, config.Content);

        string referenceFilePath = Path.Combine(ReferenceDir, fileName);
        if (!File.Exists(referenceFilePath))
        {
            MarkupLineInterpolated($"[{Orange1}]No reference file found.[/]");
            return;
        }

        string? comparison = CompareConfigs(OutputDir, ReferenceDir, fileName);
        if (string.IsNullOrWhiteSpace(comparison))
            MarkupLineInterpolated($"[{Green1}]No differences.[/]");
        else
        {
            string comparisonFile = Path.Combine(ComparisonDir, fileName);
            await File.WriteAllTextAsync(comparisonFile, comparison);
            MarkupLine($"[{Red1}]Differences found[/]");
        }
    }
}
