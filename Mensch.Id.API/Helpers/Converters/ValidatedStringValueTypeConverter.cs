using System;
using System.ComponentModel;
using System.Globalization;
using Mensch.Id.API.Helpers;

namespace Mensch.Id.API.Helpers.Converters;

public class ValidatedStringValueTypeConverter<T> : TypeConverter where T : struct, IValidatedStringValue<T>
{
    public override bool CanConvertFrom(
        ITypeDescriptorContext context,
        Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        ITypeDescriptorContext context,
        CultureInfo culture,
        object value)
    {
        if (value is string stringValue)
            return T.Parse(stringValue);
        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(
        ITypeDescriptorContext context,
        Type destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(
        ITypeDescriptorContext context,
        CultureInfo culture,
        object value,
        Type destinationType)
    {
        if (destinationType == typeof(string) && value is T typedValue)
            return typedValue.Value;
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
