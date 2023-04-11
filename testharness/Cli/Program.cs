using GitOpsConfig;

using JsonDiffPatchDotNet;

using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Json;

using static Spectre.Console.AnsiConsole;
using static Spectre.Console.Color;

const string rootDir = @"D:\Temp\config";
string appsDir = Path.Combine(rootDir, "apps");
string outputDir = Path.Combine(rootDir, "_processing", "output");
string referenceDir = Path.Combine(rootDir, "_processing", "references");

// Ensure output directory exists and delete any files in it.
if (!Directory.Exists(outputDir))
    Directory.CreateDirectory(outputDir);
string[] outputFiles = Directory.GetFiles(outputDir, "*", SearchOption.TopDirectoryOnly);
foreach (string outputFile in outputFiles)
    File.Delete(outputFile);

if (!Directory.Exists(referenceDir))
    Directory.CreateDirectory(referenceDir);

ConfigurationBuilder builder = new(rootDir);

foreach (string appName in SectionDiscoverer.EnumerateApplications(appsDir))
{
    Write(new Rule(appName).RuleStyle(new Style(Yellow1)));

    foreach (string[] sectionSet in SectionDiscoverer.DiscoverSectionsForApp(appsDir, appName))
    {
        try
        {
            await foreach (GeneratedConfiguration config in builder.GenerateAsync(appName, sectionSet))
            {
                string sectionStr = string.Join('.', sectionSet);
                string displayFileName =
                    $"[{Green1}]{appName}[/]_[{Magenta1}]{sectionStr}[/]_[{Yellow1}]{config.FileName}[/]";

                string fileName = $"{appName}_{sectionStr}_{config.FileName}";
                string filePath = Path.Combine(outputDir, fileName);

                await File.WriteAllTextAsync(filePath, config.Content);

                string? comparison = CompareConfigs(outputDir, referenceDir, fileName);
                if (comparison is null)
                {
                    MarkupLine($"[{Orange1}]No reference file - [/]{displayFileName}");
                }
                else
                {
                    JsonText jsonText = new(comparison);
                    Write(new Panel(jsonText).Header(displayFileName));
                }
            }
        }
        catch (Exception ex)
        {
            MarkupLine($"[{Red1}]Error - [/][{White}]{ex.ToString().EscapeMarkup()}[/]");
        }
    }
}

static string? CompareConfigs(string outputDir, string referenceDir, string fileName)
{
    string referenceFilePath = Path.Combine(referenceDir, fileName);
    if (!File.Exists(referenceFilePath))
        return null;

    string outputFilePath = Path.Combine(outputDir, fileName);

    JObject reference = JObject.Parse(File.ReadAllText(referenceFilePath));
    JObject output = JObject.Parse(File.ReadAllText(outputFilePath));

    JsonDiffPatch differ = new();
    JToken diff = differ.Diff(reference, output);
    return diff.ToString();
}
