using Aargh;

namespace GitOpsConfig.TestHarness.Cli;

[Command("generate", "g",
    HelpText = "Generates the configurations for the specified apps.")]
public sealed class GenerateCommand : BaseGenerateCommand
{
    protected override async ValueTask GenerateAsync(string rootDir)
    {
        string app = PromptForApp();
        string section = PromptForSection(app);
        string[] sections = section.Split('/');

        ConfigurationBuilder builder = new(rootDir);
        await foreach (GeneratedConfiguration config in builder.GenerateAsync(app, sections))
        {
            string sectionStr = string.Join('.', sections);
            string displayFileName =
                $"[{Green1}]{app}[/]_[{Magenta1}]{sectionStr}[/]_[{Yellow1}]{config.FileName}[/]";
            MarkupLine(displayFileName);

            string fileName = $"{app}_{sectionStr}_{config.FileName}";
            string filePath = Path.Combine(OutputDir, fileName);

            await File.WriteAllTextAsync(filePath, config.Content);

            string? comparison = CompareConfigs(OutputDir, ReferenceDir, fileName);
            string comparisonFile = Path.Combine(ComparisonDir, fileName);
            await File.WriteAllTextAsync(comparisonFile, comparison ?? string.Empty);
        }
    }
}
