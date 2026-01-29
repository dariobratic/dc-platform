using System.Text.RegularExpressions;
using Directory.Domain.Exceptions;

namespace Directory.Domain.ValueObjects;

public sealed partial class Slug : IEquatable<Slug>
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Slug cannot be empty.");

        var slug = value.Trim().ToLowerInvariant();

        if (slug.Length < 2 || slug.Length > 100)
            throw new DomainException("Slug must be between 2 and 100 characters.");

        if (!SlugPattern().IsMatch(slug))
            throw new DomainException("Slug must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.");

        return new Slug(slug);
    }

    public static Slug FromExisting(string value) => new(value);

    [GeneratedRegex(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")]
    private static partial Regex SlugPattern();

    public bool Equals(Slug? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Slug other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(Slug? left, Slug? right) => Equals(left, right);
    public static bool operator !=(Slug? left, Slug? right) => !Equals(left, right);
}
