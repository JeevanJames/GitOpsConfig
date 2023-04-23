using System.Text.RegularExpressions;

using IniFile;

namespace GitOpsConfig;

public sealed class VariablesBuilder : BaseBuilder
{
    public VariablesBuilder(string? rootDir)
        : base(rootDir)
    {
    }

    public async ValueTask<Variables> CollectAsync(string appName,
        IEnumerable<string> sections,
        CancellationToken cancellationToken = default)
    {
        string appDir = Path.Combine(AppsDir, appName);
        if (!Directory.Exists(appDir))
        {
            throw new DirectoryNotFoundException($"""
                Directory for app {appName} does not exist.
                It is expected at {appDir}.
                """);
        }

        // Load and resolve the variables.
        Variables variables = await AggregateAsync(appDir, sections,
            new Variables(), VariablesAggregator, cancellationToken);

        return variables;
    }

    public async ValueTask<Variables> CollectAndResolveAsync(string appName,
        IEnumerable<string> sections,
        CancellationToken cancellationToken = default)
    {
        Variables variables = await CollectAsync(appName, sections, cancellationToken);
        variables.Resolve();
        return variables;
    }

    private static ValueTask<Variables> VariablesAggregator(Variables variables, string dir, string[] sections)
    {
        string variablesFilePath = Path.Combine(dir, "variables.ini");
        if (!File.Exists(variablesFilePath))
            return ValueTask.FromResult(variables);

        Ini ini = new(new FileInfo(variablesFilePath), IniLoadSettings.ReadOnly);
        foreach (Section section in ini)
        {
            foreach (Property property in section)
            {
                string variableName = $"{section.Name}.{property.Name}";

                // Try to get the variable. If it does not exist, create it and add it to the variables
                // collection.
                if (!variables.TryGetValue(variableName, out Variable? variable))
                {
                    variable = new Variable(variableName);
                    variables.Add(variable);
                }

                // Add the current details as the initial variable source.
                variable.AddSource(new VariableSource(sections, property.Value));

                // Parse the variable value for nested variables.
                // If there are nested variables, create variables for them (if not already created)
                // and add this variable to their usages.
                MatchCollection nestedVariableMatches = Patterns.NestedVariable().Matches(property.Value);
                foreach (Match match in nestedVariableMatches)
                {
                    string nestedVariableName = match.Groups["name"].Value;

                    if (!variables.TryGetValue(nestedVariableName, out Variable? nestedVariable))
                    {
                        nestedVariable = new Variable(nestedVariableName);
                        variables.Add(nestedVariable);

                        //TODO: Since the nested variable is not created, but has been referenced,
                        //should we create a special source type for this situation? E.g. CreatedByReference
                    }

                    nestedVariable.AddUsage(new NestedVariableVariableUsage(variableName, property.Value, sections));
                }
            }
        }

        return ValueTask.FromResult(variables);
    }
}
