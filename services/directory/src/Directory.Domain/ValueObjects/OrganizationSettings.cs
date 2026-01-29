namespace Directory.Domain.ValueObjects;

public sealed class OrganizationSettings : IEquatable<OrganizationSettings>
{
    private readonly Dictionary<string, string> _values;

    public IReadOnlyDictionary<string, string> Values => _values;

    private OrganizationSettings(Dictionary<string, string> values)
    {
        _values = values;
    }

    public static OrganizationSettings Empty() => new(new Dictionary<string, string>());

    public static OrganizationSettings Create(Dictionary<string, string> values)
    {
        return new OrganizationSettings(new Dictionary<string, string>(values));
    }

    public OrganizationSettings WithSetting(string key, string value)
    {
        var copy = new Dictionary<string, string>(_values) { [key] = value };
        return new OrganizationSettings(copy);
    }

    public OrganizationSettings WithoutSetting(string key)
    {
        var copy = new Dictionary<string, string>(_values);
        copy.Remove(key);
        return new OrganizationSettings(copy);
    }

    public string? GetSetting(string key) => _values.GetValueOrDefault(key);

    public bool Equals(OrganizationSettings? other)
    {
        if (other is null) return false;
        if (_values.Count != other._values.Count) return false;
        return _values.All(kvp => other._values.TryGetValue(kvp.Key, out var val) && val == kvp.Value);
    }

    public override bool Equals(object? obj) => obj is OrganizationSettings other && Equals(other);
    public override int GetHashCode() => _values.Aggregate(0, (hash, kvp) => HashCode.Combine(hash, kvp.Key, kvp.Value));
}
