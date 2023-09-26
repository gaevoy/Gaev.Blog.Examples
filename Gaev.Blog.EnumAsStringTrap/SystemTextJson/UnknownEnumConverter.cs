using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gaev.Blog.EnumAsStringTrap.SystemTextJson;

public class UnknownEnumConverter : JsonConverterFactory
{
    private readonly JsonStringEnumConverter _underlying;

    public UnknownEnumConverter()
        => _underlying = new JsonStringEnumConverter();

    public UnknownEnumConverter(JsonNamingPolicy namingPolicy = null, bool allowIntegerValues = true)
        => _underlying = new JsonStringEnumConverter(namingPolicy, allowIntegerValues);

    public sealed override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var underlyingConverter = _underlying.CreateConverter(typeToConvert, options);
        var converterType = typeof(UnknownEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, underlyingConverter);
    }

    public sealed override bool CanConvert(Type typeToConvert)
        => _underlying.CanConvert(typeToConvert);
}

public class UnknownEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private readonly JsonConverter<T> _underlying;

    public UnknownEnumConverter(JsonConverter<T> underlying)
        => _underlying = underlying;

    public override T Read(ref Utf8JsonReader reader, Type enumType, JsonSerializerOptions options)
    {
        try
        {
            return _underlying.Read(ref reader, enumType, options);
        }
        catch (JsonException) when (enumType.IsEnum)
        {
            // TODO: Modify logic here to return custom faulty value
            return default;
        }
    }

    public override bool CanConvert(Type typeToConvert)
        => _underlying.CanConvert(typeToConvert);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => _underlying.Write(writer, value, options);

    public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => _underlying.ReadAsPropertyName(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => _underlying.WriteAsPropertyName(writer, value, options);
}
