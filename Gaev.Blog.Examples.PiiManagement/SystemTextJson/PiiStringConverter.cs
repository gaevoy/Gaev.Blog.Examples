using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gaev.Blog.Examples.PiiManagement.SystemTextJson;

public class PiiStringConverter : JsonConverter<PiiString>
{
    public override PiiString Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions __)
        => PiiScope.Serializer.FromString(reader.GetString());

    public override void Write(Utf8JsonWriter writer, PiiString value, JsonSerializerOptions _)
        => writer.WriteStringValue(PiiScope.Serializer.ToString(value));
}