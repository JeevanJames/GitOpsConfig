using Aargh;
using Aargh.Transformers;

using Spectre.Console;

namespace GitOpsConfig.TestHarness.Cli;

public abstract class BaseCommand : Command
{
    [Option("directory", "dir", "d", Optional = true,
        HelpParamName = "ROOT DIR")]
    [AsDirectory(shouldExist: true)]
    [EnvironmentFallback(0, "GITOPS_CONFIG_ROOTDIR")]
    [DefaultValueFallback(1, ".")]
    public required DirectoryInfo RootDir { get; set; }

    protected string PromptForApp()
    {
        IEnumerable<string> apps = SectionDiscoverer.EnumerateApplications(AppsDir);
        string app = Prompt(new SelectionPrompt<string>()
            .Title("Select app")
            .PageSize(10)
            .AddChoices(apps));
        MarkupLineInterpolated($"[{Cyan1}]Selected app[/]: [{Yellow1}]{app}[/]");
        return app;
    }

    protected string PromptForSection(string appName)
    {
        IEnumerable<string> sectionsSet = SectionDiscoverer.DiscoverSectionsForApp(AppsDir, appName)
            .Select(sections => string.Join('/', sections));
        string section = Prompt(new SelectionPrompt<string>()
            .Title("Select section combination")
            .PageSize(10)
            .AddChoices(sectionsSet));
        MarkupLineInterpolated($"[{Cyan1}]Selected section[/]: [{Yellow1}]{section}[/]");
        return section;
    }

    protected string AppsDir => Path.Combine(RootDir.FullName, "apps");
}
