using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

using GitOpsConfig.Internals;

namespace GitOpsConfig;

public sealed class Variables : KeyedCollection<string, Variable>
{
    private IReadOnlyDictionary<string, string>? _templateVariables;

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
                variable.UpdateResolvedValue(nestedVariablePattern.Replace(variable.Value, match =>
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
                    if (nestedVariablePattern.IsMatch(nestedVariable.Value))
                    {
                        hasNestedVariables = true;
                        return match.Value;
                    }

                    return nestedVariable.Value;
                }));
            }
        }
    }

    public IReadOnlyDictionary<string, string> GetTemplateVariables()
    {
        return _templateVariables ??=
            this.ToDictionary(v => v.Name.Replace('.', '_'), v => v.Value).AsReadOnly();
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
    private string? _value;

    public Variable(string name)
    {
        Name = name;
    }

    public string Name { get; }

    /// <summary>
    ///     Gets the final resolved value for this variable.
    /// </summary>
    public string Value => _value ?? string.Empty;

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
        string value = _value ?? CurrentUnresolvedValue;
        char valueType = _value is not null ? 'R' : 'U';
        return $"{Name} = {value} ({valueType}-S{Sources.Count}:U{Usages.Count})";
    }

    internal void InitializeResolvedValue() => _value = CurrentUnresolvedValue;

    internal void UpdateResolvedValue(string resolvedValue) => _value = resolvedValue;

    internal void AddSource(VariableSource source) => _sources.Add(source);

    internal void AddUsage(VariableUsage usage) => _usages.Add(usage);
}

/// <summary>
///     The source of a variable.
/// </summary>
/// <param name="Sections">The section that this variable was defined in.</param>
/// <param name="Value">The value of the variable at this section.</param>
public sealed record VariableSource(string[] Sections, string Value)
{
    public override string ToString() => $"{string.Join('/', Sections)} = {Value}";
}

public abstract record VariableUsage(string[] Sections);

// Variable
// Name of variable that contains this variable (referencing variable)
// The full expression of the referencing variable
public sealed record NestedVariableVariableUsage(
    string ReferencingVariableName,
    string ReferencingVariableExpression,
    string[] Sections) : VariableUsage(Sections)
{
    public override string ToString() =>
        $"Variable | {string.Join('/', Sections)} | {ReferencingVariableName} = {ReferencingVariableExpression}";
}

public sealed record FileVariableUsage(
    string FileName,
    string ContentPath,
    string[] Sections) : VariableUsage(Sections)
{
    public override string ToString() => $"File | {string.Join('/', Sections)} | {FileName}: {ContentPath}";
}
