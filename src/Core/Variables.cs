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

    /// <summary>
    ///     Gets the name of the variable.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the final resolved value for this variable.
    ///     <br/>
    ///     This value is applicable only after all variable files have been resolved.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    ///     Gets the current (unresolved) value of the variable during variable file processing.
    ///     <br/>
    ///     After the files have been processed, this value will be the one from the last-read file.
    /// </summary>
    public string CurrentUnresolvedValue => Sources.Count > 0
        ? Sources[^1].Value
        : throw new InvalidOperationException($"No sources have been added to variable {Name}.");

    /// <summary>
    ///     Gets the list of sources for this variable, i.e. the variable files where this variable
    ///     is specified.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IReadOnlyList<VariableSource> Sources => _sources;

    /// <summary>
    ///     Gets the list of usages of this variable, which includes:
    ///     <br/>
    ///     o The variable values in which this variable is mentioned as a nested variable.
    ///     o The config files where this variable is used.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public IReadOnlyList<VariableUsage> Usages => _usages;

    public bool IsUndefined => CurrentUnresolvedValue.IsSpecialValueUndefined();

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
///     Tracks information about a variable, such as its source or usage.
/// </summary>
/// <param name="Sections">The classification section that applies to this variable information.</param>
public abstract record VariableTracking(string[] Sections)
{
    public string SectionString(string separator = "/") => string.Join(separator, Sections);

    public override string ToString() => SectionString();
}

/// <summary>
///     The source of a variable.
/// </summary>
/// <param name="Sections">The classification section that this variable was defined in.</param>
/// <param name="Value">The value of the variable at this section.</param>
public sealed record VariableSource(string[] Sections, string Value) : VariableTracking(Sections);

public abstract record VariableUsage(string[] Sections) : VariableTracking(Sections);

/// <summary>
///     Usage of a variable as a nested variable within another variable's value.
/// </summary>
/// <param name="ReferencingVariableName">The name of the referencing variable.</param>
/// <param name="ReferencingVariableExpression">
///     The unresolved value of the referencing variable at the time of parsing.
/// </param>
/// <param name="Sections">The classification sections where this variables file is located.</param>
public sealed record NestedVariableVariableUsage(
    string ReferencingVariableName,
    string ReferencingVariableExpression,
    string[] Sections) : VariableUsage(Sections)
{
    public override string ToString() =>
        $"Variable | {SectionString()} | {ReferencingVariableName} = {ReferencingVariableExpression}";
}

/// <summary>
///     Usage of a variable in a config file.
/// </summary>
/// <param name="FileName">The config file name, just the name, no directory details.</param>
/// <param name="ContentPath">The path in the config file that points to the location that uses the variable.</param>
/// <param name="Sections">The classification section where this config file is located.</param>
public sealed record FileVariableUsage(
    string FileName,
    string ContentPath,
    string[] Sections) : VariableUsage(Sections)
{
    public override string ToString() => $"File | {SectionString()} | {FileName}: {ContentPath}";
}
