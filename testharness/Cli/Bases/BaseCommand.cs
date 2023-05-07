namespace GitOpsConfig.TestHarness.Cli.Bases;

public abstract class BaseCommand : Command
{
    [Option("directory", "dir", "d", Optional = true,
        HelpParamName = "ROOT DIR",
        HelpText = "Root directory for the configuration.")]
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
            .MoreChoicesText("Use arrow keys to see more options")
            .AddChoices(apps));
        MarkupLineInterpolated($"[{Cyan1}]Selected app[/]: [{Yellow1}]{app}[/]");
        return app;
    }

    protected string PromptForSection(string appName)
    {
        IEnumerable<string> sectionSets = SectionDiscoverer.DiscoverSectionsForApp(AppsDir, appName)
            .Select(sections => string.Join('/', sections));
        string section = Prompt(new SelectionPrompt<string>()
            .Title("Select section combination")
            .PageSize(10)
            .MoreChoicesText("Use arrow keys to see more options")
            .AddChoices(sectionSets));
        MarkupLineInterpolated($"[{Cyan1}]Selected section[/]: [{Yellow1}]{section}[/]");
        return section;
    }

    protected IEnumerable<string> PromptForSections(string appName)
    {
        IEnumerable<string> sectionSets = SectionDiscoverer.DiscoverSectionsForApp(AppsDir, appName)
            .Select(sections => string.Join('/', sections));
        IEnumerable<string> selectedSections = Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select sections")
                .PageSize(10)
                .MoreChoicesText("Use arrow keys to see more options")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle an option, " +
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(sectionSets));

        MarkupLineInterpolated($"[{Cyan1}]Selected sections[/]: [{Yellow1}]{string.Join(", ", selectedSections)}[/]");
        return selectedSections;
    }

    protected string AppsDir => Path.Combine(RootDir.FullName, "apps");
}
