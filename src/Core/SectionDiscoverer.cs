namespace GitOpsConfig;

public static class SectionDiscoverer
{
    public static IEnumerable<string[]> DiscoverSectionsFrom(string appsDir)
    {
        IEnumerable<string> appDirs = Directory.EnumerateDirectories(appsDir, "*", SearchOption.TopDirectoryOnly);
        foreach (string appDir in appDirs)
        {
            IEnumerable<string[]> appSections = DiscoverSectionsForApp(appDir);
            foreach (string[] appSection in appSections)
                yield return appSection;
        }
    }

    public static IEnumerable<string[]> DiscoverSectionsForApp(string appDir)
    {
        List<string[]> chains = new();

        foreach (string subdir in Directory.EnumerateDirectories(appDir, "*", SearchOption.TopDirectoryOnly))
        {
            EnumerateSubdir(subdir, chains, new Stack<string>());
        }

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
        {
            chains.Add(currentChain.Reverse().ToArray());
            currentChain.Pop();
        }
    }
}
