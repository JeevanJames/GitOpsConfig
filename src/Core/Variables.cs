using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitOpsConfig;

public sealed class Variables : KeyedCollection<string, Variable>
{
    public Variables()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    public void Resolve()
    {
        // Ensure that there are no undefined variables.
        IEnumerable<string> undefinedVariables = this
            .Where(v => v.IsUndefined)
            .Select(v => v.Name);
        if (undefinedVariables.Any())
        {
            throw new InvalidOperationException($"""
                The following variables are still undefined.
                {string.Join(", ", undefinedVariables)}.
                """);
        }

        foreach (Variable variable in this)
            variable.InitializeResolvedValue();

        Regex nestedVariablePattern = Patterns.NestedVariable();

        bool hasNestedVariables = true;
        while (hasNestedVariables)
        {
            hasNestedVariables = false;
            foreach (Variable variable in this)
            {
                variable.UpdateResolvedValue(nestedVariablePattern.Replace(variable.ResolvedValue, match =>
                {
                    string nestedVariableKey = match.Groups["name"].Value;

                    // If the nested variable key does not exist, throw an exception.
                    if (!TryGetValue(nestedVariableKey, out Variable? nestedVariable))
                    {
                        throw new InvalidOperationException(
                            $"Variable {variable.Name} specifies a nested variable {nestedVariableKey}, which does not exist.");
                    }

                    // If the nested variable value has its own nested variables, then don't resolve
                    // them now.
                    if (nestedVariablePattern.IsMatch(nestedVariable.ResolvedValue))
                    {
                        hasNestedVariables = true;
                        nestedVariable.AddUsage(new NestedVariableVariableUsage(
                            variable.Name, variable.CurrentUnresolvedValue));
                        return match.Value;
                    }

                    return nestedVariable.ResolvedValue;
                }));
            }
        }
    }

    protected override string GetKeyForItem(Variable item)
    {
        return item.Name;
    }
}

public sealed class Variable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly List<VariableSource> _sources = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly List<VariableUsage> _usages = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _resolvedValue;

    public Variable(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string ResolvedValue => _resolvedValue ?? string.Empty;

    public string CurrentUnresolvedValue => Sources.Count > 0
        ? Sources[^1].Value
        : throw new InvalidOperationException($"No sources have been added to variable {Name}.");

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IReadOnlyList<VariableSource> Sources => _sources;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IReadOnlyList<VariableUsage> Usages => _usages;

    public bool IsUndefined => string.Equals(CurrentUnresolvedValue, "_UNDEFINED_", StringComparison.Ordinal);

    public override string ToString()
    {
        string value = _resolvedValue ?? CurrentUnresolvedValue;
        string valueType = _resolvedValue is not null ? "[R]" : "[U]";
        return $"{Name} = {valueType} {value} (S{Sources.Count}, U{Usages.Count})";
    }

    internal void InitializeResolvedValue() => _resolvedValue = CurrentUnresolvedValue;

    internal void UpdateResolvedValue(string resolvedValue) => _resolvedValue = resolvedValue;

    internal void AddSource(VariableSource source) => _sources.Add(source);

    internal void AddUsage(VariableUsage usage) => _usages.Add(usage);
}

public sealed record VariableSource(string SourcePath, string Value);

public abstract record VariableUsage;

// Variable
// Name of variable that contains this variable (referencing variable)
// The full expression of the referencing variable
public sealed record NestedVariableVariableUsage(
    string ReferencingVariableName,
    string ReferencingVariableExpression) : VariableUsage
{
    public override string ToString() =>
        $"Variable | {ReferencingVariableName} = {ReferencingVariableExpression}";
}

public sealed record FileVariableUsage(
    string FileName,
    string ContentPath) : VariableUsage
{
    public override string ToString() => $"File | {FileName}: {ContentPath}";
}
