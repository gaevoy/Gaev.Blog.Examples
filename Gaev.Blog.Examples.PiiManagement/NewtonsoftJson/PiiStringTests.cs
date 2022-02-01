using System;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Gaev.Blog.Examples.PiiManagement.NewtonsoftJson;

public class PiiStringTests
{
    [Test]
    public void NewtonsoftJson_should_work()
    {
        // Given
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        var settings = new JsonSerializerSettings { Converters = { new PiiStringConverter() } };

        // When
        var json = JsonConvert.SerializeObject(user, settings);
        var actual = JsonConvert.DeserializeObject<User>(json, settings);

        // Then
        Console.WriteLine(json);
        actual.Should().BeEquivalentTo(user);
    }

    [Test]
    public void NewtonsoftJson_should_encrypt()
    {
        // Given
        using var _ = new PiiScope(new PiiSerializers.Aes128WithRandomIv("hb50qBZSF0fcLSl9814PIqmO4gEZcJGB/Kd4fpTTBcU="));
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        var settings = new JsonSerializerSettings { Converters = { new PiiStringConverter() } };

        // When
        var json = JsonConvert.SerializeObject(user, settings);
        var actual = JsonConvert.DeserializeObject<User>(json, settings);

        // Then
        Console.WriteLine(json);
        actual.Should().BeEquivalentTo(user);
    }

    [Test]
    public void NewtonsoftJson_should_hash()
    {
        // Given
        using var _ = new PiiScope(new PiiSerializers.Sha256());
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        var settings = new JsonSerializerSettings { Converters = { new PiiStringConverter() } };

        // When
        var json = JsonConvert.SerializeObject(user, settings);
        var json2 = JsonConvert.SerializeObject(user, settings);
        var json3 = JsonConvert.SerializeObject(user, settings);

        // Then
        Console.WriteLine(json);
        Console.WriteLine(json2);
        Console.WriteLine(json3);
    }
}