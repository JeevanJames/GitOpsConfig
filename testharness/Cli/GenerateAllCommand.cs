using Aargh;

using Spectre.Console;
using Spectre.Console.Json;

namespace GitOpsConfig.TestHarness.Cli;

[Command("generate-all", "ga",
    HelpText = "Generates all the configurations for all apps in the specified root directory.")]
public sealed class GenerateAllCommand : BaseGenerateCommand
{
    protected override async ValueTask GenerateAsync(string rootDir)
    {
        ConfigurationBuilder builder = new(rootDir);

        foreach (string appName in SectionDiscoverer.EnumerateApplications(AppsDir))
        {
            Write(new Rule(appName).RuleStyle(new Style(Yellow1)));

            foreach (string[] sectionSet in SectionDiscoverer.DiscoverSectionsForApp(AppsDir, appName))
            {
                try
                {
                    await foreach (GeneratedConfiguration config in builder.GenerateAsync(appName, sectionSet))
                    {
                        string sectionStr = string.Join('.', sectionSet);
                        string displayFileName =
                            $"[{Green1}]{appName}[/]_[{Magenta1}]{sectionStr}[/]_[{Yellow1}]{config.FileName}[/]";
                        MarkupLine(displayFileName);

                        string fileName = $"{appName}_{sectionStr}_{config.FileName}";
                        string filePath = Path.Combine(OutputDir, fileName);

                        await File.WriteAllTextAsync(filePath, config.Content);

                        string? comparison = CompareConfigs(OutputDir, ReferenceDir, fileName);
                        string comparisonFile = Path.Combine(ComparisonDir, fileName);
                        await File.WriteAllTextAsync(comparisonFile, comparison ?? string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    MarkupLine($"[{Red1}]Error - [/][{White}]{ex.ToString().EscapeMarkup()}[/]");
                }
            }
        }
    }
}
