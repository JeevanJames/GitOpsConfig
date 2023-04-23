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

    private static ValueTask<Variables> VariablesAggregator(Variables variables, string dir)
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

                if (!variables.TryGetValue(variableName, out Variable? variable))
                {
                    variable = new Variable(variableName);
                    variables.Add(variable);
                }

                variable.AddSource(new VariableSource(dir, property.Value));
            }
        }

        return ValueTask.FromResult(variables);
    }
}
