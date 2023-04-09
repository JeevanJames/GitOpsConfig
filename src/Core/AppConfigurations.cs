namespace GitOpsConfig;

public sealed class AppConfigurations
{
    internal AppConfigurations(string appName, IReadOnlyList<GeneratedConfiguration> configurations)
    {
        AppName = appName;
        Configurations = configurations;
    }

    public string AppName { get; }

    public IReadOnlyList<GeneratedConfiguration> Configurations { get; }

    public void Deconstruct(out string appName, out IReadOnlyList<GeneratedConfiguration> configurations)
    {
        appName = AppName;
        configurations = Configurations;
    }
}
