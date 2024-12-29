using System.Text.Json;

namespace JrEncoder;

public static class JsonExtensions
{
    public static T Get<T>(this JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement valueElement))
            throw new JsonException($"Property {propertyName} not found.");

        Type type = typeof(T);

        if (type == typeof(byte))
            return (T)(object)valueElement.GetByte();

        if (type == typeof(int))
            return (T)(object)valueElement.GetInt32();

        if (type == typeof(long))
            return (T)(object)valueElement.GetInt64();

        if (type == typeof(float))
            return (T)(object)valueElement.GetSingle();

        if (type == typeof(double))
            return (T)(object)valueElement.GetDouble();

        if (type == typeof(bool))
            return (T)(object)valueElement.GetBoolean();

        if (type == typeof(string))
            return (T)(object)valueElement.GetString()!;

        throw new JsonException($"Type of property {propertyName} not supported.");
    }

    public static string Raw(this JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement valueElement))
            throw new JsonException($"Property {propertyName} not found.");

        string text = valueElement.GetRawText();
        return text[1..(text.Length - 1)];
    }
}