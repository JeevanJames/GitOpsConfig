using System.Linq.Expressions;

using GitOpsConfig;

using JsonDiffPatchDotNet;

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

Table table = new Table()
    .AddColumns("File name", "Comparison");

List<string> differences = new();

foreach (string appName in SectionDiscoverer.EnumerateApplications(appsDir))
{
    IEnumerable<string[]> sectionSets = SectionDiscoverer.DiscoverSectionsForApp(appsDir, appName);
    foreach (string[] sectionSet in sectionSets)
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

                bool? compareResult = CompareConfigs(outputDir, referenceDir, fileName);
                string result = compareResult switch
                {
                    null => $"[{Orange1}]Reference file not found[/]",
                    true => $"[{Green1}]Semantically equal[/]",
                    false => $"[{Yellow4}]Different[/]",
                };
                if (!compareResult.GetValueOrDefault(true))
                    differences.Add(filePath);

                table.AddRow(
                    new Markup(displayFileName),
                    new Markup(result));
            }
        }
        catch (Exception ex)
        {
            table.AddRow(
                new Markup($"[{Green1}]{appName}[/]_[{Magenta1}]{string.Join('.', sectionSet)}[/]"),
                new Markup($"[{Red1}]{ex.Message.EscapeMarkup()}[/]"));
        }
    }
}

Write(table);

foreach (string difference in differences)
{
    string diff = CompareConfigs2(outputDir, referenceDir, Path.GetFileName(difference));
    MarkupLineInterpolated($"[{Blue1}]{difference}[/]");
    System.Console.WriteLine(diff);
}

System.Console.ReadLine();

static bool? CompareConfigs(string outputDir, string referenceDir, string fileName)
{
    string referenceFilePath = Path.Combine(referenceDir, fileName);
    if (!File.Exists(referenceFilePath))
        return null;

    string outputFilePath = Path.Combine(outputDir, fileName);

    JObject reference = JObject.Parse(File.ReadAllText(referenceFilePath));
    JObject output = JObject.Parse(File.ReadAllText(outputFilePath));

    return JToken.DeepEquals(output, reference);
}

static string CompareConfigs2(string outputDir, string referenceDir, string fileName)
{
    string referenceFilePath = Path.Combine(referenceDir, fileName);
    if (!File.Exists(referenceFilePath))
        return string.Empty;

    string outputFilePath = Path.Combine(outputDir, fileName);

    JObject reference = JObject.Parse(File.ReadAllText(referenceFilePath));
    JObject output = JObject.Parse(File.ReadAllText(outputFilePath));

    JsonDiffPatch differ = new();
    JToken diff = differ.Diff(reference, output);
    return diff.ToString();
}
