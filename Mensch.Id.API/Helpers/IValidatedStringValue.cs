namespace Mensch.Id.API.Helpers;

public interface IValidatedStringValue<TSelf> where TSelf : struct, IValidatedStringValue<TSelf>
{
    string Value { get; }
    static abstract TSelf Parse(string value);
}
