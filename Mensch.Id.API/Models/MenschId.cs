using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Helpers.Converters;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models;

[JsonConverter(typeof(ValidatedStringValueJsonConverter<MenschId>))]
[TypeConverter(typeof(ValidatedStringValueTypeConverter<MenschId>))]
public readonly partial struct MenschId : IEquatable<MenschId>, IValidatedStringValue<MenschId>
{
    public const string SuffixCharacters = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";
    public const int SuffixLength = 5;

    public MenschId(string value)
    {
        var normalizedValue = value?.Trim().ToUpperInvariant();
        if (normalizedValue == null || !ValidFormatRegex().IsMatch(normalizedValue))
            throw new ArgumentException($"Invalid mensch.ID '{value}'. Expected format: yyyyMMdd-XXXXX", nameof(value));
        Value = normalizedValue;
    }

    public string Value { get; }

    public static MenschId Parse(string value)
    {
        return new MenschId(value);
    }

    public static bool IsValid(string value)
    {
        var normalizedValue = value?.Trim().ToUpperInvariant();
        return normalizedValue != null && ValidFormatRegex().IsMatch(normalizedValue);
    }

    public bool Equals(MenschId other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is MenschId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(MenschId left, MenschId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MenschId left, MenschId right)
    {
        return !left.Equals(right);
    }

    [GeneratedRegex(@"^[0-9]{8}-[ACDEFGHJKLMNPQRSTUVWXYZ2345679]{5}\z")]
    private static partial Regex ValidFormatRegex();
}
