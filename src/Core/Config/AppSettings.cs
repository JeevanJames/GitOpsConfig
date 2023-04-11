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
        private TypeTransformConfigModel? _typeTransforms;

        public string Name
        {
            get => _name ?? throw new InvalidOperationException("Configuration file name cannot be null.");
            set => _name = value ?? throw new InvalidOperationException("Configuration file name cannot be null.");
        }

        public TypeTransformConfigModel TypeTransforms
        {
            get => _typeTransforms ??= new TypeTransformConfigModel();
            set => _typeTransforms = value;
        }
    }

    internal sealed class TypeTransformConfigModel
    {
        private List<string>? _boolean;
        private List<string>? _number;

        public List<string> Boolean
        {
            get => _boolean ??= new List<string>();
            set => _boolean = value;
        }

        public List<string> Number
        {
            get => _number ??= new List<string>();
            set => _number = value;
        }
    }
}
