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
                        await HandleConfig(config, appName, sectionSet);
                }
                catch (Exception ex)
                {
                    MarkupLine($"[{Red1}]Error - [/][{White}]{ex.ToString().EscapeMarkup()}[/]");
                }
            }
        }
    }
}
