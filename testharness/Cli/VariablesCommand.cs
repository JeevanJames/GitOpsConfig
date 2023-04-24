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
        string section = PromptForSections(app).First();

        VariablesBuilder builder = new(RootDir.FullName);
        string[] sections = section.Split('/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Variables variables = Resolve
            ? await builder.CollectAndResolveAsync(app, sections)
            : await builder.CollectAsync(app, sections);

        Table table = new Table()
            .AddColumns("Section", "Variable", "Value");

        foreach (Variable variable in variables.OrderBy(v => v.Name))
        {
            string[] nameParts = variable.Name.Split('.', 2,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string value = Resolve ? variable.Value : variable.CurrentUnresolvedValue;
            Markup valueMarkup;
            if (string.IsNullOrWhiteSpace(value))
                valueMarkup = new Markup("[EMPTY/WHITESPACE]".EscapeMarkup(), new Style(Orange1));
            else if (value == "_UNDEFINED_")
                valueMarkup = new Markup("UNDEFINED", new Style(Red1));
            else
                valueMarkup = new Markup(value.EscapeMarkup(), new Style(Green1));

            table.AddRow(
                new Markup(nameParts[0], new Style(Cyan1)),
                new Markup(nameParts[1], new Style(Yellow1)),
                valueMarkup);
        }

        Write(table);
    }
}
