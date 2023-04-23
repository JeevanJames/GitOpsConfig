using System.Text.RegularExpressions;

namespace GitOpsConfig;

internal static partial class Patterns
{
    [GeneratedRegex(@"\$\((?<name>\w+(\.\w+)+)\)",
        RegexOptions.ExplicitCapture | RegexOptions.Compiled, 1000)]
    internal static partial Regex NestedVariable();
}
