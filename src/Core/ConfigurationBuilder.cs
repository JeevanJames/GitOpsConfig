﻿using System.Text.RegularExpressions;

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

    public async IAsyncEnumerable<GeneratedConfiguration> GenerateAsync(string appName, params string[] sections)
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
        Settings settings = await LoadSettings(appDir, appName);

        // Load and resolve the variables.
        Dictionary<string, string> variables = Aggregate(appDir, sections,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            VariablesAggregator);
        ResolveVariables(variables);

        foreach (string configFileName in settings.Files)
        {
            JObject? accumulate = Aggregate(appDir, sections,
                (JObject?)null,
                (acc, dir) => JsonAggregator(acc, dir, configFileName));

            if (accumulate is not null)
            {
                ResolveJsonValues(accumulate, variables);
                yield return new GeneratedConfiguration(configFileName, accumulate.ToString());
            }
        }
    }

    public async Task<IDictionary<string, IList<GeneratedConfiguration>>> GenerateAsync(
        Func<string, bool> predicate,
        IEnumerable<string> sections,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<string> appNames = Directory
            .EnumerateDirectories(_appsDir, "*", SearchOption.TopDirectoryOnly)
            .Select(dir => Path.GetFileName(dir))
            .Where(predicate);

        Dictionary<string, IList<GeneratedConfiguration>> result = new(StringComparer.OrdinalIgnoreCase);

        foreach (string appName in appNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<GeneratedConfiguration> configs = await GenerateAsync(appName, sections.ToArray()).ToListAsync();
            result.Add(appName, configs);
        }

        return result;
    }

    public async Task<IDictionary<string, IList<GeneratedConfiguration>>> GenerateAsync(
        IEnumerable<string> sections,
        CancellationToken cancellationToken = default) =>
        await GenerateAsync(_ => true, sections, cancellationToken);

    private static async Task<Settings> LoadSettings(string appDir, string appName)
    {
        string settingsFilePath = Path.Combine(appDir, "__settings.json");
        Settings? settings = File.Exists(settingsFilePath)
            ? JsonConvert.DeserializeObject<Settings>(await File.ReadAllTextAsync(settingsFilePath))
            : new Settings();

        if (settings is null)
            throw new InvalidOperationException($"Error reading from settings file for {appName} from {settingsFilePath}.");

        if (settings.Files.Count == 0)
        {
            throw new InvalidOperationException(
                $"App {appName} settings file at {settingsFilePath} does not specify any configuration files.");
        }

        return settings;
    }

    private TAccumulate Aggregate<TAccumulate>(string appDir, IList<string> sections,
        TAccumulate seed,
        Func<TAccumulate, string, TAccumulate> aggregatorFunc)
    {
        TAccumulate accumulate = seed;

        string dir = _sharedDir;
        accumulate = aggregatorFunc(accumulate, dir);
        foreach (string section in sections)
        {
            dir = Path.Combine(dir, section);
            if (!Directory.Exists(dir))
                break;
            accumulate = aggregatorFunc(accumulate, dir);
        }

        dir = appDir;
        accumulate = aggregatorFunc(accumulate, dir);
        foreach (string section in sections)
        {
            dir = Path.Combine(dir, section);
            if (!Directory.Exists(dir))
                break;
            accumulate = aggregatorFunc(accumulate, dir);
        }

        return accumulate;
    }

    private static Dictionary<string, string> VariablesAggregator(Dictionary<string, string> variables, string dir)
    {
        string variablesFilePath = Path.Combine(dir, "variables.ini");
        if (!File.Exists(variablesFilePath))
            return variables;

        Ini ini = new(new FileInfo(variablesFilePath), IniLoadSettings.ReadOnly);
        foreach (Section section in ini)
        {
            foreach (Property property in section)
            {
                string variableName = $"{section.Name}.{property.Name}";
                variables[variableName] = property.Value;
            }
        }

        return variables;
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

    private static JObject? JsonAggregator(JObject? accumulate, string dir, string fileName)
    {
        string jsonFilePath = Path.Combine(dir, fileName);
        if (!File.Exists(jsonFilePath))
            return accumulate;

        accumulate ??= new JObject();

        JObject jsonObj = JObject.Parse(File.ReadAllText(jsonFilePath));
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
                foreach (JProperty property in jobject.Properties())
                    ResolveJsonValues(property.Value, variables);
                break;

            case JArray jarray:
                foreach (JToken item in jarray)
                    ResolveJsonValues(item, variables);
                break;

            default:
                throw new InvalidOperationException($"Unrecognized token type - {token.GetType()}");
        }
    }

    [GeneratedRegex(@"\$\((?<name>\w+(\.\w+)+)\)",
        RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking, 1000)]
    private static partial Regex NestedVariablePattern();
}
