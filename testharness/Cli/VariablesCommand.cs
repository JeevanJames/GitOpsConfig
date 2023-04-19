using Aargh;

using Spectre.Console;

namespace GitOpsConfig.TestHarness.Cli;

[Command("variables", "v",
    HelpText = "Displays all variables for a specified app and sections.")]
public sealed class VariablesCommand : BaseCommand
{
    [Flag("resolve", "r")]
    public bool Resolve { get; set; }

    public override async ValueTask HandleCommandAsync(IParseResult parseResult)
    {
        string app = PromptForApp();
        string section = PromptForSection(app);

        VariablesBuilder builder = new(RootDir.FullName);
        string[] sections = section.Split('/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IDictionary<string, string> variables = Resolve
            ? await builder.CollectAndResolveAsync(app, sections)
            : await builder.CollectAsync(app, sections);

        Table table = new Table()
            .AddColumns("Section", "Variable", "Value");

        foreach (string name in variables.Keys.OrderBy(k => k))
        {
            string[] nameParts = name.Split('.', 2,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string value = variables[name];
            Markup valueMarkup = value == "_UNDEFINED_"
                ? new("UNDEFINED", new Style(Red1))
                : new(value, new Style(Green1));
            table.AddRow(
                new Markup(nameParts[0], new Style(Cyan1)),
                new Markup(nameParts[1], new Style(Yellow1)),
                valueMarkup);
        }

        Write(table);
    }
}
