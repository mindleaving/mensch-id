using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Helpers.Converters;
using Mensch.Id.API.Storage;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Mensch.Id.API.Models;

[JsonConverter(typeof(ValidatedStringValueJsonConverter<LoginProvider>))]
[BsonSerializer(typeof(LoginProviderBsonSerializer))]
[TypeConverter(typeof(ValidatedStringValueTypeConverter<LoginProvider>))]
public readonly partial struct LoginProvider : IEquatable<LoginProvider>, IValidatedStringValue<LoginProvider>
{
    public static readonly LoginProvider Local = new("local");

    public LoginProvider(string value)
    {
        if (value == null || !ValidFormatRegex().IsMatch(value))
            throw new ArgumentException($"Invalid login provider '{value}'. Only letters and digits are allowed.", nameof(value));
        Value = value;
    }

    public string Value { get; }

    public static LoginProvider Parse(string value)
    {
        return new LoginProvider(value);
    }

    public static bool IsValid(string value)
    {
        return value != null && ValidFormatRegex().IsMatch(value);
    }

    public bool Equals(LoginProvider other)
    {
        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is LoginProvider other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(LoginProvider left, LoginProvider right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LoginProvider left, LoginProvider right)
    {
        return !left.Equals(right);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9]+\z")]
    private static partial Regex ValidFormatRegex();
}
