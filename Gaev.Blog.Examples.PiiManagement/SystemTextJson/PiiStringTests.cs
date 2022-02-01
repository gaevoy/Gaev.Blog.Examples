using System;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace Gaev.Blog.Examples.PiiManagement.SystemTextJson;

public class PiiStringTests
{
    [Test]
    public void SystemTextJson_should_work()
    {
        // Given
        var user = new User
        {
            Name = "John Doe",
            Email = "john.doe@test.com"
        };
        var settings = new JsonSerializerOptions { Converters = { new PiiStringConverter() } };

        // When
        var json = JsonSerializer.Serialize(user, settings);
        var actual = JsonSerializer.Deserialize<User>(json, settings);

        // Then
        Console.WriteLine(json);
        actual.Should().BeEquivalentTo(user);
    }
}