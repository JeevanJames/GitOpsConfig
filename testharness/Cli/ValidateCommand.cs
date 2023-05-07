using GitOpsConfig.TestHarness.Cli.Bases;

namespace GitOpsConfig.TestHarness.Cli;

[Command("validate",
    HelpText = "Performs various validations on variables.")]
public sealed class ValidateCommand : BaseCommand
{
    public override async ValueTask HandleCommandAsync(IParseResult parseResult)
    {
        string app = PromptForApp();
        string section = PromptForSection(app);

        ConfigurationBuilder builder = new(RootDir.FullName);
        await foreach (GeneratedConfiguration config in builder.GenerateAsync(app, section.Split('/')))
        {
            Variables variables = config.Variables;

            // Variables with no usage, but have no shared sources.
            // This means they were specifically created for the app, but have no usages in the app.
            IEnumerable<Variable> vars = variables
                .Where(v => v.Usages.Count == 0 && v.Sources.All(s => s.Sections[0] != "shared"))
                .OrderBy(v => v.Name);
            foreach (Variable v in vars)
                MarkupLine(v.Name);
        }
    }
}
