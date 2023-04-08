using GitOpsConfig;

ConfigurationBuilder builder = new(@"D:\Temp\config");
IEnumerable<GeneratedConfiguration> configs = builder.Generate("MyApp", "dev");
foreach ((string fileName, string content) in configs)
{
    Console.WriteLine(fileName);
    Console.WriteLine(content);
}
