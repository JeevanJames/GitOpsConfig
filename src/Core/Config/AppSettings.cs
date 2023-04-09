namespace GitOpsConfig.Config;

internal sealed class AppSettings
{
    private List<FileConfigModel>? _files;

    public List<FileConfigModel> Files
    {
        get => _files ??= new List<FileConfigModel>();
        set => _files = value;
    }

    internal sealed class FileConfigModel
    {
        private string? _name;

        public string Name
        {
            get => _name ?? throw new InvalidOperationException("Configuration file name cannot be null.");
            set => _name = value ?? throw new InvalidOperationException("Configuration file name cannot be null.");
        }
    }
}
