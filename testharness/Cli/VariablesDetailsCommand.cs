namespace GitOpsConfig.TestHarness.Cli;

[Command<VariablesCommand>("details", "d",
    HelpText = "Displays details of a specific variable.")]
public sealed class VariablesDetailsCommand : BaseCommand
{
    [Argument(Order = 0,
        HelpName = "VARIABLE",
        HelpText = "Full name of the variable to get details for.")]
    public required string VariableName { get; set; }

    public override async ValueTask HandleCommandAsync(IParseResult parseResult)
    {
        string app = PromptForApp();
        string section = PromptForSection(app);

        ConfigurationBuilder builder = new(RootDir.FullName);
        GeneratedConfiguration? config = await builder.GenerateAsync(app, section.Split('/'))
            .FirstOrDefaultAsync();

        if (config is null)
            throw new InvalidOperationException($"No configurations generated for {app}, {section}");

        if (!config.Variables.TryGetValue(VariableName, out Variable? variable))
            throw new InvalidOperationException($"Cannot find variable {VariableName}.");

        MarkupLineInterpolated($"[{Cyan1}]Sources:[/]");
        foreach (VariableSource source in variable.Sources)
            MarkupLineInterpolated($"    [{Yellow1}]{source.SectionString()}[/] - [{CadetBlue_1}]{source.Value}[/]");

        MarkupLineInterpolated($"[{Cyan1}]Variable Usages:[/]");
        foreach (NestedVariableVariableUsage usage in variable.Usages.OfType<NestedVariableVariableUsage>())
            MarkupLineInterpolated($"    [{Yellow1}]{usage.SectionString()}[/] - [{White}]{usage.ReferencingVariableName}[/] = [{CadetBlue_1}]{usage.ReferencingVariableExpression}[/]");
        MarkupLineInterpolated($"[{Cyan1}]File Usages:[/]");
        foreach (FileVariableUsage usage in variable.Usages.OfType<FileVariableUsage>())
            MarkupLineInterpolated($"    [{Yellow1}]{usage.SectionString()}[/] - [{White}]{usage.FileName}[/] => [{CadetBlue_1}]{usage.ContentPath}[/]");
    }
}
