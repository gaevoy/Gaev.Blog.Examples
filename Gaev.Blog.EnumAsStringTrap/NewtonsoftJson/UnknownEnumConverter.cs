﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gaev.Blog.EnumAsStringTrap.NewtonsoftJson;

public class UnknownEnumConverter : StringEnumConverter
{
    // https://stackoverflow.com/a/51847437
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch (JsonSerializationException) when (objectType.IsEnum)
        {
            // TODO: Modify logic here to return custom faulty value
            // This returns default value https://stackoverflow.com/a/353073
            return Activator.CreateInstance(objectType);
        }
    }
}
