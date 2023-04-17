using System.Text.RegularExpressions;

using IniFile;

namespace GitOpsConfig;

public sealed class VariablesBuilder : BaseBuilder
{
    public VariablesBuilder(string? rootDir)
        : base(rootDir)
    {
    }

    public async ValueTask<IDictionary<string, string>> CollectAsync(string appName,
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
        Dictionary<string, string> variables = await AggregateAsync(appDir, sections,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            VariablesAggregator, cancellationToken);

        return variables;
    }

    public async ValueTask<IDictionary<string, string>> CollectAndResolveAsync(string appName,
        IEnumerable<string> sections,
        CancellationToken cancellationToken = default)
    {
        IDictionary<string, string> variables = await CollectAsync(appName, sections, cancellationToken);
        ResolveVariables(variables);
        return variables;
    }

    private static ValueTask<Dictionary<string, string>> VariablesAggregator(
        Dictionary<string, string> variables,
        string dir)
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
                variables[variableName] = property.Value;
            }
        }

        return ValueTask.FromResult(variables);
    }

    private static void ResolveVariables(IDictionary<string, string> variables)
    {
        // Ensure that there are no undefined variables.
        IEnumerable<string> undefinedVariables = variables
            .Where(v => v.Value.Equals("_UNDEFINED_", StringComparison.Ordinal))
            .Select(v => v.Key);
        if (undefinedVariables.Any())
        {
            throw new InvalidOperationException($"""
                The following variables are still undefined.
                {string.Join(", ", undefinedVariables)}.
                """);
        }

        Regex nestedVariablePattern = NestedVariablePattern();

        bool hasNestedVariables = true;
        while (hasNestedVariables)
        {
            hasNestedVariables = false;
            foreach (string key in variables.Keys.ToArray())
            {
                string value = variables[key];

                variables[key] = nestedVariablePattern.Replace(value, match =>
                {
                    string nestedVariableKey = match.Groups["name"].Value;

                    // If the nested variable key does not exist, throw an exception.
                    if (!variables.TryGetValue(nestedVariableKey, out string? nestedVariableValue))
                    {
                        throw new InvalidOperationException(
                            $"Variable {key} specifies a nested variable {nestedVariableKey}, which does not exist.");
                    }

                    // If the nested variable value has its own nested variables, then don't resolve
                    // them now.
                    if (nestedVariablePattern.IsMatch(nestedVariableValue))
                    {
                        hasNestedVariables = true;
                        return match.Value;
                    }

                    return nestedVariableValue;
                });
            }
        }
    }
}
