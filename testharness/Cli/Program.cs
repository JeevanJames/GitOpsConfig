using GitOpsConfig;

using IniFile;

using Spectre.Console;

using static Spectre.Console.AnsiConsole;
using static Spectre.Console.Color;

const string rootDir = @"D:\Temp\config";
string appsDir = Path.Combine(rootDir, "apps");
string outputDir = Path.Combine(rootDir, "_processing", "output");

if (!Directory.Exists(outputDir))
    Directory.CreateDirectory(outputDir);

ConfigurationBuilder builder = new(rootDir);

IEnumerable<string[]> sectionSets = SectionDiscoverer.DiscoverSectionsFrom(appsDir);

Table table = new Table()
    .AddColumns("App", "Sections", "Result");

foreach (string[] sectionSet in sectionSets)
{
    string sectionPath = $"[{Cyan1}]{string.Join($"[{Magenta1}] -> [/]", sectionSet)}[/]";

    string currentAppName = "N/A";
    try
    {
        await foreach ((string appName, IReadOnlyList<GeneratedConfiguration> configs) in
                       builder.GenerateAsync(sectionSet))
        {
            currentAppName = appName;

            foreach (GeneratedConfiguration config in configs)
            {
                string sectionStr = string.Join('.', sectionSet);
                string fileName = $"{currentAppName}_{sectionStr}_{config.FileName}";
                string filePath = Path.Combine(outputDir, fileName);
                await File.WriteAllTextAsync(filePath, config.Content);

                table.AddRow(currentAppName, sectionPath, $"[{Green1}]{fileName.EscapeMarkup()}[/]");
            }
        }
    }
    catch (Exception ex)
    {
        table.AddRow(
            new Text(currentAppName),
            new Markup(sectionPath),
            ex.GetRenderable(ExceptionFormats.ShortenEverything));
    }
}

Write(table);

System.Console.ReadLine();
