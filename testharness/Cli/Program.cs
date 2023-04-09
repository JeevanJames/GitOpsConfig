using GitOpsConfig;

using Spectre.Console;
using Spectre.Console.Json;

using static Spectre.Console.AnsiConsole;
using static Spectre.Console.Color;

ConfigurationBuilder builder = new(@"D:\Temp\config");

//await foreach ((string fileName, string content) in builder.GenerateAsync("MyApp", "dev"))
//{
//    Write(new Rule(fileName));
//    Write(new JsonText(content));
//}

await foreach ((string appName, IReadOnlyList<GeneratedConfiguration> configs)
    in builder.GenerateAsync(new[] { "dev" }))
{
    Write(new Rule(appName).RuleStyle(new Style(Yellow1)));
    foreach (GeneratedConfiguration config in configs)
    {
        Write(new Panel(new JsonText(config.Content))
            .Header(config.FileName)
            .BorderColor(Cyan1));
    }
}

System.Console.ReadLine();
