namespace GitOpsConfig;

internal sealed class Settings
{
    private List<string>? _files;

    public List<string> Files
    {
        get => _files ??= new List<string>();
        set => _files = value;
    }
}
