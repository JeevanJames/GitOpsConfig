using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using GitOpsConfig.Config;

using IniFile;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitOpsConfig;

public sealed partial class ConfigurationBuilder
{
    private readonly string _sharedDir;
    private readonly string _appsDir;

    public ConfigurationBuilder(string? rootDir)
    {
        rootDir = Path.GetFullPath(rootDir ?? ".");
        if (!Directory.Exists(rootDir))
            throw new DirectoryNotFoundException($"Root directory {rootDir} not found.");

        _appsDir = Path.Combine(rootDir, "apps");
        if (!Directory.Exists(_appsDir))
            throw new DirectoryNotFoundException($"Apps directory {_appsDir} not found.");

        _sharedDir = Path.Combine(rootDir, "shared");
        if (!Directory.Exists(_sharedDir))
            throw new DirectoryNotFoundException($"Shared directory {_sharedDir} not found.");
    }

    public async IAsyncEnumerable<GeneratedConfiguration> GenerateAsync(string appName,
        IEnumerable<string> sections,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string appDir = Path.Combine(_appsDir, appName);
        if (!Directory.Exists(appDir))
        {
            throw new DirectoryNotFoundException($"""
                Directory for app {appName} does not exist.
                It is expected at {appDir}.
                """);
        }

        // Load the settings file and deal with any errors in it.
        AppSettings settings = await LoadSettings(appDir, appName);

        // Load and resolve the variables.
        Dictionary<string, string> variables = await AggregateAsync(appDir, sections,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            VariablesAggregator, cancellationToken);
        ResolveVariables(variables);

        foreach (AppSettings.FileConfigModel fileConfig in settings.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            JObject? accumulate = await AggregateAsync<JObject?>(appDir, sections, null,
                (acc, dir) => JsonAggregator(acc, dir, fileConfig.Name), cancellationToken);

            if (accumulate is not null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ResolveJsonValues(accumulate, variables);

                string configStr = accumulate.ToString();

                // Ensure that there are no variables in the final configuration string.
                MatchCollection variableMatches = NestedVariablePattern().Matches(configStr);
                if (variableMatches.Count > 0)
                {
                    string variablesFound = string.Join(", ",
                        variableMatches.Take(15).Select(m => m.Value));
                    throw new InvalidOperationException($"""
                        Found {variableMatches.Count} variable(s) in the final JSON configuration.
                        These need to be resolved.
                        The variables include {variablesFound}.
                        """);
                }

                yield return new GeneratedConfiguration(fileConfig.Name, configStr);
            }
        }
    }

    public async IAsyncEnumerable<AppConfigurations> GenerateAsync(
        Func<string, bool> predicate,
        IEnumerable<string> sections,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IEnumerable<string> appNames = Directory
            .EnumerateDirectories(_appsDir, "*", SearchOption.TopDirectoryOnly)
            .Select(dir => Path.GetFileName(dir))
            .Where(predicate);

        foreach (string appName in appNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<GeneratedConfiguration> configs = await GenerateAsync(appName,
                    sections.ToArray(),
                    cancellationToken)
                .ToListAsync(cancellationToken);
            yield return new AppConfigurations(appName, configs);
        }
    }

    public IAsyncEnumerable<AppConfigurations> GenerateAsync(
        IEnumerable<string> sections,
        CancellationToken cancellationToken = default) =>
        GenerateAsync(_ => true, sections, cancellationToken);

    private static async Task<AppSettings> LoadSettings(string appDir, string appName)
    {
        string settingsFilePath = Path.Combine(appDir, "__settings.json");
        AppSettings? settings = File.Exists(settingsFilePath)
            ? JsonConvert.DeserializeObject<AppSettings>(await File.ReadAllTextAsync(settingsFilePath))
            : new AppSettings();

        if (settings is null)
            throw new InvalidOperationException($"Error reading from settings file for {appName} from {settingsFilePath}.");

        if (settings.Files.Count == 0)
        {
            throw new InvalidOperationException(
                $"App {appName} settings file at {settingsFilePath} does not specify any configuration files.");
        }

        return settings;
    }

    private async ValueTask<TAccumulate> AggregateAsync<TAccumulate>(string appDir,
        IEnumerable<string> sections,
        TAccumulate seed,
        Func<TAccumulate, string, ValueTask<TAccumulate>> aggregatorFunc,
        CancellationToken cancellationToken)
    {
        TAccumulate accumulate = seed;

        string dir = _sharedDir;
        cancellationToken.ThrowIfCancellationRequested();
        accumulate = await aggregatorFunc(accumulate, dir);
        foreach (string section in sections)
        {
            dir = Path.Combine(dir, section);
            if (!Directory.Exists(dir))
                break;

            cancellationToken.ThrowIfCancellationRequested();
            accumulate = await aggregatorFunc(accumulate, dir);
        }

        //TODO: If we hit the last section, then ensure that there are no further subdirectories

        dir = appDir;
        cancellationToken.ThrowIfCancellationRequested();
        accumulate = await aggregatorFunc(accumulate, dir);
        foreach (string section in sections)
        {
            dir = Path.Combine(dir, section);
            if (!Directory.Exists(dir))
                break;

            cancellationToken.ThrowIfCancellationRequested();
            accumulate = await aggregatorFunc(accumulate, dir);
        }

        //TODO: If we hit the last section, then ensure that there are no further subdirectories

        return accumulate;
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

    private static void ResolveVariables(Dictionary<string, string> variables)
    {
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

    private static async ValueTask<JObject?> JsonAggregator(JObject? accumulate, string dir, string fileName)
    {
        string jsonFilePath = Path.Combine(dir, fileName);
        if (!File.Exists(jsonFilePath))
            return accumulate;

        accumulate ??= new JObject();

        JObject jsonObj = JObject.Parse(await File.ReadAllTextAsync(jsonFilePath));
        accumulate.Merge(jsonObj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            MergeNullValueHandling = MergeNullValueHandling.Ignore,
        });

        return accumulate;
    }

    private static void ResolveJsonValues(JToken token, Dictionary<string, string> variables)
    {
        switch (token)
        {
            case JValue jvalue:
                string? value = jvalue.Value<string>();
                if (value is not null && NestedVariablePattern().IsMatch(value))
                {
                    value = NestedVariablePattern().Replace(value, match =>
                    {
                        string nestedVariableKey = match.Groups["name"].Value;

                        // If the nested variable key does not exist, throw an exception.
                        if (!variables.TryGetValue(nestedVariableKey, out string? nestedVariableValue))
                        {
                            throw new InvalidOperationException(
                                $"Nested variable key {nestedVariableKey} does not exist.");
                        }

                        // If the nested variable value has its own nested variables, then don't resolve
                        // them now.
                        if (NestedVariablePattern().IsMatch(nestedVariableValue))
                        {
                            throw new InvalidOperationException(
                                $"Variable {nestedVariableKey} has unresolved nested variables.");
                        }

                        return nestedVariableValue;
                    });

                    jvalue.Replace(new JValue(value));
                }

                break;

            case JObject jobject:
                JProperty[] properties = jobject.Properties().ToArray();
                for (int i = 0; i < properties.Length; i++)
                    ResolveJsonValues(properties[i].Value, variables);
                break;

            case JArray jarray:
                for (int i = 0; i < jarray.Count; i++)
                    ResolveJsonValues(jarray[i], variables);
                break;

            default:
                throw new InvalidOperationException($"Unrecognized token type - {token.GetType()}");
        }
    }

    [GeneratedRegex(@"\$\((?<name>\w+(\.\w+)+)\)",
        RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking | RegexOptions.Compiled, 1000)]
    private static partial Regex NestedVariablePattern();
}
