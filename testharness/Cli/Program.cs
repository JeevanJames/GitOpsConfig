using System.Linq.Expressions;

using GitOpsConfig;

using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Json.Syntax;

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

IEnumerable<string[]> sectionSets = SectionDiscoverer.DiscoverSectionsFrom(appsDir);

Table table = new Table()
    .AddColumns("File name", "Comparison");

foreach (string[] sectionSet in sectionSets)
{
    try
    {
        await foreach ((string appName, IReadOnlyList<GeneratedConfiguration> configs) in
                       builder.GenerateAsync(sectionSet))
        {
            foreach (GeneratedConfiguration config in configs)
            {
                string sectionStr = string.Join('.', sectionSet);
                string displayFileName = $"[{Green1}]{appName}[/]_[{Magenta1}]{sectionStr}[/]_[{Yellow1}]{config.FileName}[/]";

                string fileName = $"{appName}_{sectionStr}_{config.FileName}";
                string filePath = Path.Combine(outputDir, fileName);

                await File.WriteAllTextAsync(filePath, config.Content);

                string result = CompareConfigs(outputDir, referenceDir, fileName) switch
                {
                    null => $"[{Orange1}]Reference file not found[/]",
                    true => $"[{Green1}]Semantically equal[/]",
                    false => $"[{Yellow4}]Different[/]",
                };

                table.AddRow(
                    new Markup(displayFileName),
                    new Markup(result));
            }
        }
    }
    catch (Exception ex)
    {
        table.AddRow(
            new Markup(string.Join('.', sectionSet)),
            new Markup($"[{Red1}]{ex.Message}[/]"));
    }
}

Write(table);

System.Console.ReadLine();

static bool? CompareConfigs(string outputDir, string referenceDir, string fileName)
{
    string referenceFilePath = Path.Combine(referenceDir, referenceDir);
    if (!File.Exists(referenceFilePath))
        return null;

    string outputFilePath = Path.Combine(outputDir, fileName);

    JObject reference = JObject.Parse(File.ReadAllText(referenceFilePath));
    JObject output = JObject.Parse(File.ReadAllText(outputFilePath));

    return JToken.DeepEquals(output, reference);
}
