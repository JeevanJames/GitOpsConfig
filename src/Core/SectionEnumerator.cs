namespace GitOpsConfig;

public static class SectionEnumerator
{
    public static IEnumerable<string[]> EnumerateSectionsFrom(string rootDir)
    {
        List<string[]> chains = new();

        foreach (string subdir in Directory.EnumerateDirectories(rootDir, "*", SearchOption.TopDirectoryOnly))
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
