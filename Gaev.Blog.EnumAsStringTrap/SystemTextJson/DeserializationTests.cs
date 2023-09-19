using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace Gaev.Blog.EnumAsStringTrap.SystemTextJson;

public class DeserializationTests
{
    [Test]
    public void Deserialize_all_known_enum_values()
    {
        // Given
        var json = """
                   [
                       {"Type": "Invoice", "Id": 1},
                       {"Type": "CreditNote", "Id": 2}
                   ]
                   """;

        // When
        var actual = Deserialize(json);

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Document(DocumentType.Invoice, 1),
            new Document(DocumentType.CreditNote, 2)
        });
    }

    [Test]
    public void Deserialize_unknown_enum_value()
    {
        // Given
        var json = """
                   [
                       {"Type": "Invoice", "Id": 1},
                       {"Type": "CreditNote", "Id": 2},
                       {"Type": "Order", "Id": 3}
                   ]
                   """;

        // When
        var actual = Deserialize(json);

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Document(DocumentType.Invoice, 1),
            new Document(DocumentType.CreditNote, 2)
        });
    }

    [Test]
    public void Deserialize_unknown_enum_value_server_fix()
    {
        // Given
        var json = """
                   [
                       {"Type": 1, "Id": 1},
                       {"Type": 2, "Id": 2},
                       {"Type": 3, "Id": 3}
                   ]
                   """;

        // When
        var actual = Deserialize(json);

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Document(DocumentType.Invoice, 1),
            new Document(DocumentType.CreditNote, 2),
            new Document((DocumentType)3, 3),
        });
    }

    [Test]
    public void Deserialize_unknown_enum_value_client_fix()
    {
        // Given
        var json = """
                   [
                       {"Type": "Invoice", "Id": 1},
                       {"Type": "CreditNote", "Id": 2},
                       {"Type": "Order", "Id": 3}
                   ]
                   """;

        // When
        var actual = JsonSerializer.Deserialize<Document[]>(json, new JsonSerializerOptions
        {
            Converters = { new UnknownEnumConverter() }
        });

        //Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Document(DocumentType.Invoice, 1),
            new Document(DocumentType.CreditNote, 2),
            new Document(default, 3),
        });
    }

    private static Document[] Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Document[]>(json, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        });
    }
}
