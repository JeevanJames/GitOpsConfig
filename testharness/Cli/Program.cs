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

IDictionary<string, IList<GeneratedConfiguration>> results = await builder.GenerateAsync(new[] { "dev" });
foreach ((string appName, IList<GeneratedConfiguration> configs) in results)
{
    Write(new Rule(appName));
    foreach (GeneratedConfiguration config in configs)
    {
        Write(new Panel(new JsonText(config.Content))
            .Header(config.FileName));
    }
}

System.Console.ReadLine();
