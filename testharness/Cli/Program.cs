using GitOpsConfig;

using IniFile;

using Spectre.Console;
using Spectre.Console.Json;

using static Spectre.Console.AnsiConsole;
using static Spectre.Console.Color;

const string rootDir = @"D:\Temp\config";
string appsDir = Path.Combine(rootDir, "apps");

//ConfigurationBuilder builder = new(rootDir);

//await foreach ((string fileName, string content) in builder.GenerateAsync("MyApp", "dev"))
//{
//    Write(new Rule(fileName));
//    Write(new JsonText(content));
//}

//await foreach ((string appName, IReadOnlyList<GeneratedConfiguration> configs)
//    in builder.GenerateAsync(new[] { "qa", "in-qa" }))
//{
//    Write(new Rule(appName).RuleStyle(new Style(Yellow1)));
//    foreach (GeneratedConfiguration config in configs)
//    {
//        JsonText jsonText = new JsonText(config.Content)
//            .BracketColor(Yellow3)
//            .BracesColor(Yellow1);
//        Write(new Panel(jsonText)
//            .Header(config.FileName)
//            .BorderColor(Cyan1)
//            .DoubleBorder()
//            .Expand());
//    }
//}

IEnumerable<string[]> sectionSets = SectionDiscoverer.DiscoverSectionsFrom(appsDir);
foreach (string[] section in sectionSets)
{
    string path = string.Join($"[{Yellow1}]/[/]", section);
    MarkupLine($"[{Green1}]{path}[/]");
}

System.Console.ReadLine();
