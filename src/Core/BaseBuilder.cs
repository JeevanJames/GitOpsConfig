using System.Text.RegularExpressions;

using GitOpsConfig.Config;

using Newtonsoft.Json;

namespace GitOpsConfig;

public abstract partial class BaseBuilder
{
    protected BaseBuilder(string? rootDir)
    {
        RootDir = Path.GetFullPath(rootDir ?? ".");
        if (!Directory.Exists(RootDir))
            throw new DirectoryNotFoundException($"Root directory {RootDir} not found.");

        AppsDir = Path.Combine(RootDir, "apps");
        if (!Directory.Exists(AppsDir))
            throw new DirectoryNotFoundException($"Apps directory {AppsDir} not found.");

        SharedDir = Path.Combine(RootDir, "shared");
        if (!Directory.Exists(SharedDir))
            throw new DirectoryNotFoundException($"Shared directory {SharedDir} not found.");
    }

    public string RootDir { get; }

    public string SharedDir { get; }

    public string AppsDir { get; }

    protected async ValueTask<TAccumulate> AggregateAsync<TAccumulate>(string appDir,
        IEnumerable<string> sections,
        TAccumulate seed,
        Func<TAccumulate, string, ValueTask<TAccumulate>> aggregatorFunc,
        CancellationToken cancellationToken)
    {
        TAccumulate accumulate = seed;

        string dir = SharedDir;
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

    protected static async Task<AppSettings> LoadSettings(string appDir, string appName)
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

    [GeneratedRegex(@"\$\((?<name>\w+(\.\w+)+)\)",
        RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking | RegexOptions.Compiled, 1000)]
    protected static partial Regex NestedVariablePattern();
}
