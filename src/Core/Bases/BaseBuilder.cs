using GitOpsConfig.Config;

using Newtonsoft.Json;

namespace GitOpsConfig.Bases;

/// <summary>
///     Base class for building something from the config repository directory structure.
///     <br/>
///     Deriving classes can use the <see cref="AggregateAsync{TAccumulate}"/> method to build the
///     final value by iterating over the config repository directories in the correct sequence.
/// </summary>
public abstract class BaseBuilder
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

    /// <summary>
    ///     Gets the root directory of the config repository.
    /// </summary>
    public string RootDir { get; }

    /// <summary>
    ///     Gets the shared directory of the config repository.
    /// </summary>
    public string SharedDir { get; }

    /// <summary>
    ///     Gets the root apps directory of the config repository.
    /// </summary>
    public string AppsDir { get; }

    /// <summary>
    ///     Given a seed value, iterates over the config repository directories in the correct sequence
    ///     (shared subdirectory first and then the specific app subdirectory under apps) and aggregates
    ///     a final value.
    /// </summary>
    /// <typeparam name="TAccumulate">The type of value to aggregate.</typeparam>
    /// <param name="appDir">The specific app sub-directory.</param>
    /// <param name="sections">The classification sections to iterate over.</param>
    /// <param name="seed">The initial seed value to build the aggregate from.</param>
    /// <param name="aggregatorFunc">Delegate called on every directory iterated, to aggregate the value.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The final aggregated value.</returns>
    protected async ValueTask<TAccumulate> AggregateAsync<TAccumulate>(string appDir,
        IEnumerable<string> sections,
        TAccumulate seed,
        Func<TAccumulate, string, string[], ValueTask<TAccumulate>> aggregatorFunc,
        CancellationToken cancellationToken)
    {
        TAccumulate accumulate = seed;

        cancellationToken.ThrowIfCancellationRequested();

        string dir = SharedDir;
        List<string> currentSections = new(new[] { "shared" });
        accumulate = await aggregatorFunc(accumulate, dir, currentSections.ToArray());
        foreach (string section in sections)
        {
            dir = Path.Combine(dir, section);
            if (!Directory.Exists(dir))
                break;

            currentSections.Add(section);

            accumulate = await aggregatorFunc(accumulate, dir, currentSections.ToArray());
        }

        //TODO: If we hit the last section, then ensure that there are no further subdirectories

        cancellationToken.ThrowIfCancellationRequested();

        dir = appDir;
        currentSections.Clear();
        currentSections.Add(Path.GetFileName(appDir));
        accumulate = await aggregatorFunc(accumulate, dir, currentSections.ToArray());
        foreach (string section in sections)
        {
            dir = Path.Combine(dir, section);
            if (!Directory.Exists(dir))
                break;

            currentSections.Add(section);

            accumulate = await aggregatorFunc(accumulate, dir, currentSections.ToArray());
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
}
