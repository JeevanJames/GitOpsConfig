using Aargh;
using Aargh.Transformers;

using Spectre.Console;

namespace GitOpsConfig.TestHarness.Cli;

[Command("variables", "v")]
public sealed class VariablesCommand : Command
{
    [Argument(Order = 0)]
    public required string AppName { get; set; }

    [Argument(Order = 1)]
    public required string Sections { get; set; }

    [Option("directory", "dir", "d", Optional = true)]
    [AsDirectory(shouldExist: true)]
    [DefaultValueFallback(".")]
    public required DirectoryInfo Directory { get; set; }

    [Flag("resolve", "r")]
    public bool Resolve { get; set; }

    public override async ValueTask HandleCommandAsync(IParseResult parseResult)
    {
        VariablesBuilder builder = new(Directory.FullName);
        string[] sections = Sections.Split(',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IDictionary<string, string> variables = Resolve
            ? await builder.CollectAndResolveAsync(AppName, sections)
            : await builder.CollectAsync(AppName, sections);

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
