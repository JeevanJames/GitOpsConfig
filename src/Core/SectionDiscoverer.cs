namespace GitOpsConfig;

public static class SectionDiscoverer
{
    public static IEnumerable<string> EnumerateApplications(string appsDir)
    {
        return Directory.EnumerateDirectories(appsDir, "*", SearchOption.TopDirectoryOnly)
            .Select(dir => Path.GetFileName(dir));
    }

    public static IEnumerable<string[]> DiscoverSectionsFrom(string appsDir)
    {
        foreach (string appDir in Directory.EnumerateDirectories(appsDir, "*", SearchOption.TopDirectoryOnly))
        {
            foreach (string[] appSection in DiscoverSectionsForApp(appsDir, Path.GetFileName(appDir)))
                yield return appSection;
        }
    }

    public static IEnumerable<string[]> DiscoverSectionsForApp(string appsDir, string appName)
    {
        List<string[]> chains = new();

        string appDir = Path.Combine(appsDir, appName);
        foreach (string subdir in Directory.EnumerateDirectories(appDir, "*", SearchOption.TopDirectoryOnly))
            EnumerateSubdir(subdir, chains, new Stack<string>());

        return chains;
    }

    private static void EnumerateSubdir(string dir, List<string[]> chains, Stack<string> currentChain)
    {
        currentChain.Push(Path.GetFileName(dir));

        IEnumerable<string> subdirs = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly);
        if (subdirs.Any())
        {
            foreach (string subdir in subdirs)
                EnumerateSubdir(subdir, chains, currentChain);
        }
        else
            chains.Add(currentChain.Reverse().ToArray());

        currentChain.Pop();
    }
}
