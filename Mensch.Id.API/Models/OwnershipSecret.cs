using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Helpers.Converters;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models;

[JsonConverter(typeof(ValidatedStringValueJsonConverter<OwnershipSecret>))]
[TypeConverter(typeof(ValidatedStringValueTypeConverter<OwnershipSecret>))]
public readonly partial struct OwnershipSecret : IEquatable<OwnershipSecret>, IValidatedStringValue<OwnershipSecret>
{
    public const string AllowedCharacters = "ACDEFGHJKLMNPQRSTUVWXYZ2345679";
    public const int Length = 30;

    public OwnershipSecret(string value)
    {
        if (value == null || !ValidFormatRegex().IsMatch(value))
            throw new ArgumentException($"Invalid ownership secret. Expected {Length} characters from '{AllowedCharacters}'", nameof(value));
        Value = value;
    }

    public string Value { get; }

    public static OwnershipSecret Parse(string value)
    {
        return new OwnershipSecret(value);
    }

    public static bool IsValid(string value)
    {
        return value != null && ValidFormatRegex().IsMatch(value);
    }

    public static OwnershipSecret Generate()
    {
        var value = RandomNumberGenerator.GetString(AllowedCharacters, Length);
        return new OwnershipSecret(value);
    }

    public bool Equals(OwnershipSecret other)
    {
        if (Value == null || other.Value == null)
            return Value == null && other.Value == null;
        var valueBytes = Encoding.UTF8.GetBytes(Value);
        var otherValueBytes = Encoding.UTF8.GetBytes(other.Value);
        return CryptographicOperations.FixedTimeEquals(valueBytes, otherValueBytes);
    }

    public override bool Equals(object obj)
    {
        return obj is OwnershipSecret other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(OwnershipSecret left, OwnershipSecret right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OwnershipSecret left, OwnershipSecret right)
    {
        return !left.Equals(right);
    }

    [GeneratedRegex(@"^[ACDEFGHJKLMNPQRSTUVWXYZ2345679]{30}\z")]
    private static partial Regex ValidFormatRegex();
}
