using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace Gaev.Blog.EnumAsStringTrap.SystemTextJson;

public class DeserializationTests
{
    [Test]
    public void It_should_deserialize_all_known_enum_values()
    {
        // Given
        var json = """
                   [
                       {"Currency": "EUR", "Amount": 1},
                       {"Currency": "USD", "Amount": 2}
                   ]
                   """;

        // When
        var actual = Deserialize(json);

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Money(Currency.EUR, 1),
            new Money(Currency.USD, 2)
        });
    }

    [Test]
    public void It_should_deserialize_unknown_enum_value()
    {
        // Given
        var json = """
                   [
                       {"Currency": "EUR", "Amount": 1},
                       {"Currency": "USD", "Amount": 2},
                       {"Currency": "Bitcoin", "Amount": 3}
                   ]
                   """;

        // When
        var actual = Deserialize(json);

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Money(Currency.EUR, 1),
            new Money(Currency.USD, 2)
        });
    }

    [Test]
    public void It_should_deserialize_unknown_enum_value_server_fix()
    {
        // Given
        var json = """
                   [
                       {"Currency": 1, "Amount": 1},
                       {"Currency": 2, "Amount": 2},
                       {"Currency": 3, "Amount": 3}
                   ]
                   """;

        // When
        var actual = Deserialize(json);

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Money(Currency.EUR, 1),
            new Money(Currency.USD, 2),
            new Money((Currency)3, 3),
        });
    }

    [Test]
    public void It_should_deserialize_unknown_enum_value_client_fix()
    {
        // Given
        var json = """
                   [
                       {"Currency": "EUR", "Amount": 1},
                       {"Currency": "USD", "Amount": 2},
                       {"Currency": "Bitcoin", "Amount": 3}
                   ]
                   """;

        // When
        var actual = JsonSerializer.Deserialize<Money[]>(json, new JsonSerializerOptions
        {
            Converters = { new UnknownEnumConverter() }
        });

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Money(Currency.EUR, 1),
            new Money(Currency.USD, 2),
            new Money(default, 3),
        });
    }

    private static Money[] Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Money[]>(json, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        });
    }
}
