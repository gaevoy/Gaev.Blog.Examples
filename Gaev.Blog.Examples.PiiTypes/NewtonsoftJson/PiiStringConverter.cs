using System;
using Newtonsoft.Json;

namespace Gaev.Blog.Examples.NewtonsoftJson;

public class PiiStringConverter : JsonConverter<PiiString>
{
    private readonly IPiiEncoder _encoder;

    public PiiStringConverter(IPiiEncoder encoder)
        => _encoder = encoder;

    public override PiiString ReadJson(JsonReader reader, Type _, PiiString __, bool ___, JsonSerializer ____)
        => reader.Value is string valueAsString
            ? _encoder.ToPiiString(valueAsString)
            : null;

    public override void WriteJson(JsonWriter writer, PiiString value, JsonSerializer _)
        => writer.WriteValue(_encoder.ToSystemString(value));
}