namespace GitOpsConfig.TestHarness.Cli;

[Command("generate", "g",
    HelpText = "Generates the configurations for the specified apps.")]
public sealed class GenerateCommand : BaseGenerateCommand
{
    protected override async ValueTask GenerateAsync(string rootDir)
    {
        string app = PromptForApp();
        string section = PromptForSection(app);
        string[] sections = section.Split('/');

        ConfigurationBuilder builder = new(rootDir);
        await foreach (GeneratedConfiguration config in builder.GenerateAsync(app, sections))
            await HandleConfig(config, app, sections);
    }
}
