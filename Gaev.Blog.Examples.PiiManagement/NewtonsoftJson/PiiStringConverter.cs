using System;
using Newtonsoft.Json;

namespace Gaev.Blog.Examples.PiiManagement.NewtonsoftJson;

public class PiiStringConverter : JsonConverter<PiiString>
{
    public override PiiString ReadJson(JsonReader reader, Type _, PiiString __, bool ___, JsonSerializer ____)
        => reader.Value is string valueAsString
            ? PiiScope.Serializer.FromString(valueAsString)
            : null;

    public override void WriteJson(JsonWriter writer, PiiString value, JsonSerializer _)
        => writer.WriteValue(PiiScope.Serializer.ToString(value));
}