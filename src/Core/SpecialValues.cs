using System.Runtime.CompilerServices;

namespace GitOpsConfig;

public static class SpecialValues
{
    public const string Undefined = "_UNDEFINED_";
    public const string Null = "_NULL_";
    public const string Empty = "_EMPTY_";

    public static bool IsSpecialValueUndefined(this string value) =>
        string.Equals(value, Undefined, StringComparison.Ordinal);

    public static bool IsSpecialValueNull(this string value) =>
        string.Equals(value, Null, StringComparison.Ordinal);

    public static bool IsSpecialValueEmpty(this string value) =>
        string.Equals(value, Empty, StringComparison.Ordinal);
}
