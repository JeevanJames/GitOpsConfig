﻿namespace GitOpsConfig;

public sealed class GeneratedConfiguration : IEquatable<GeneratedConfiguration>
{
    internal GeneratedConfiguration(string fileName, string content)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public string FileName { get; }

    public string Content { get; }

    public bool Equals(GeneratedConfiguration? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return string.Equals(FileName, other.FileName, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || (obj is GeneratedConfiguration other && Equals(other));

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(FileName);

    public static bool operator ==(GeneratedConfiguration? left, GeneratedConfiguration? right) =>
        Equals(left, right);

    public static bool operator !=(GeneratedConfiguration? left, GeneratedConfiguration? right) =>
        !Equals(left, right);

    public void Deconstruct(out string fileName, out string content)
    {
        fileName = FileName;
        content = Content;
    }
}
