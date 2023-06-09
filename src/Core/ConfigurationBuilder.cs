﻿using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using GitOpsConfig.Bases;
using GitOpsConfig.Config;
using GitOpsConfig.Internals;

using Newtonsoft.Json.Linq;

using Scriban;

namespace GitOpsConfig;

public sealed class ConfigurationBuilder : BaseBuilder
{
    public ConfigurationBuilder(string? rootDir)
        : base(rootDir)
    {
    }

    public async IAsyncEnumerable<GeneratedConfiguration> GenerateAsync(string appName,
        IEnumerable<string> sections,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string appDir = Path.Combine(AppsDir, appName);
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
        VariablesBuilder variablesBuilder = new(RootDir);
        Variables variables = await variablesBuilder
            .CollectAndResolveAsync(appName, sections, cancellationToken);

        // Process each file for this app.
        foreach (AppSettings.FileConfigModel fileConfig in settings.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Go through the directory hierarchy and merge the json files.
            JObject? accumulate = await AggregateAsync<JObject?>(appDir, sections, null,
                (acc, dir, s) => JsonAggregator(acc, dir, s, fileConfig, variables), cancellationToken);

            if (accumulate is null)
                continue;

            // Resolve nested variables in the JSON values.
            ResolveJsonValues(accumulate, variables, fileConfig);

            // Convert any JSON values to booleans or numbers if they are configured as such.
            UpdateDataTypes(accumulate, fileConfig);

            // Convert the JSON object into a string.
            string configStr = accumulate.ToString();

            // Ensure that there are no variables in the final configuration string.
            MatchCollection variableMatches = Patterns.NestedVariable().Matches(configStr);
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

            yield return new GeneratedConfiguration(fileConfig.Name, configStr, variables);
        }
    }

    public async IAsyncEnumerable<AppConfigurations> GenerateAsync(
        Func<string, bool> predicate,
        IEnumerable<string> sections,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IEnumerable<string> appNames = Directory
            .EnumerateDirectories(AppsDir, "*", SearchOption.TopDirectoryOnly)
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

    private static async ValueTask<JObject?> JsonAggregator(JObject? accumulate, string dir, string[] sections,
        AppSettings.FileConfigModel fileConfig,
        Variables variables)
    {
        string jsonFilePath = Path.Combine(dir, fileConfig.Name);
        if (!File.Exists(jsonFilePath))
            return accumulate;

        accumulate ??= new JObject();

        // Read the contents of the file.
        string fileContent = await File.ReadAllTextAsync(jsonFilePath);

        // If the file is configured as a template, resolve the template.
        if (fileConfig.IsTemplate)
        {
            Template template = Template.Parse(fileContent);
            if (template.HasErrors)
            {
                StringBuilder errorMessage = new StringBuilder($"Error in template '{jsonFilePath}'.").AppendLine();
                for (int i = 0; i < template.Messages.Count; i++)
                    errorMessage.AppendLine($"{i}. {template.Messages[i].Message}");
                throw new InvalidOperationException(errorMessage.ToString());
            }

            fileContent = await template.RenderAsync(variables.GetTemplateVariables());
        }

        // Load the original file content or the resolved template content.
        // If the template resolution results in invalid JSON, then it should fail here with an exception.
        JObject jsonObj = JObject.Parse(fileContent);

        // Update the variable usages for just this file under this section.
        // Do this before merging, so that the updates are isolated to only the current JSON file.
        UpdateVariableUsages(jsonObj, variables, fileConfig, sections);

        accumulate.Merge(jsonObj, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            MergeNullValueHandling = MergeNullValueHandling.Ignore,
        });

        return accumulate;
    }

    private static void UpdateVariableUsages(JToken token, Variables variables,
        AppSettings.FileConfigModel fileConfig, string[] sections)
    {
        switch (token)
        {
            case JValue jvalue:
                string? value = jvalue.Value<string>();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    MatchCollection nestedVariableMatches = Patterns.NestedVariable().Matches(value);
                    foreach (Match match in nestedVariableMatches)
                    {
                        string nestedVariableName = match.Groups["name"].Value;
                        if (!variables.TryGetValue(nestedVariableName, out Variable? nestedVariable))
                        {
                            throw new InvalidOperationException($"""
                                Found a nested variable in JSON file named '{nestedVariableName}'
                                at location '{jvalue.Path}'.
                                This variable has not been declared in any of the variable files.
                                """);
                        }

                        nestedVariable.AddUsage(new FileVariableUsage(fileConfig.Name, jvalue.Path, sections));
                    }
                }

                break;

            case JObject jobject:
                JProperty[] properties = jobject.Properties().ToArray();
                for (int i = 0; i < properties.Length; i++)
                    UpdateVariableUsages(properties[i].Value, variables, fileConfig, sections);
                break;

            case JArray jarray:
                for (int i = 0; i < jarray.Count; i++)
                    UpdateVariableUsages(jarray[i], variables, fileConfig, sections);
                break;

            default:
                throw new InvalidOperationException($"Unrecognized token type - {token.GetType()}");
        }
    }

    private static void ResolveJsonValues(JToken token, Variables variables,
        AppSettings.FileConfigModel fileConfig)
    {
        switch (token)
        {
            case JValue jvalue:
                string? value = jvalue.Value<string>();
                if (value is not null && Patterns.NestedVariable().IsMatch(value))
                {
                    value = Patterns.NestedVariable().Replace(value, (MatchEvaluator)(match =>
                    {
                        string nestedVariableKey = match.Groups["name"].Value;

                        // If the nested variable key does not exist, throw an exception.
                        if (!variables.TryGetValue(nestedVariableKey, out Variable? nestedVariable))
                        {
                            throw new InvalidOperationException(
                                $"Nested variable key {nestedVariableKey} does not exist.");
                        }

                        // If the nested variable value has its own nested variables, then don't resolve
                        // them now.
                        if (Patterns.NestedVariable().IsMatch(nestedVariable.Value))
                        {
                            throw new InvalidOperationException(
                                $"Variable {nestedVariableKey} has unresolved nested variables.");
                        }

                        return nestedVariable.Value;
                    }));

                    jvalue.Replace(new JValue(value));
                }

                break;

            case JObject jobject:
                JProperty[] properties = jobject.Properties().ToArray();
                for (int i = 0; i < properties.Length; i++)
                    ResolveJsonValues(properties[i].Value, variables, fileConfig);
                break;

            case JArray jarray:
                for (int i = 0; i < jarray.Count; i++)
                    ResolveJsonValues(jarray[i], variables, fileConfig);
                break;

            default:
                throw new InvalidOperationException($"Unrecognized token type - {token.GetType()}");
        }
    }

    private static void UpdateDataTypes(JObject jobject, AppSettings.FileConfigModel settings)
    {
        Replace(settings.Name, jobject, settings.TypeTransforms.Boolean, s =>
            bool.TryParse(s, out bool boolValue)
                ? new JValue(boolValue)
                : throw new InvalidOperationException("Not a boolean."));

        Replace(settings.Name, jobject, settings.TypeTransforms.Number, s =>
        {
            if (long.TryParse(s, out long integralValue))
                return new JValue(integralValue);
            if (double.TryParse(s, out double floatingPointValue))
                return new JValue(floatingPointValue);
            throw new InvalidOperationException("Not a number.");
        });

        static void Replace(string fileName, JObject jo, IList<string> paths, Func<string, JValue> converter)
        {
            foreach (string path in paths)
            {
                JToken? matchedToken = jo.SelectToken(path);
                if (matchedToken is null)
                    continue;

                if (matchedToken is not JValue jvalue)
                {
                    throw new InvalidOperationException($"""
                        Matched a data type path {path} in file {fileName} to {matchedToken.Path}.
                        However, this is not a value ({nameof(JValue)}), but is a {matchedToken.GetType().Name} instead.
                        """);
                }

                JValue transformedValue = converter(jvalue.ToString());
                jvalue.Replace(transformedValue);
            }
        }
    }
}
