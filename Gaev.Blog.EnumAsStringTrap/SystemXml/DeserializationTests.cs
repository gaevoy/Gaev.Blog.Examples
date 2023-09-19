using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FluentAssertions;
using NUnit.Framework;

namespace Gaev.Blog.EnumAsStringTrap.SystemXml;

public class DeserializationTests
{
    [Test]
    public void Deserialize_all_known_enum_values()
    {
        // Given
        var xml = """
                  <ArrayOfDocument>
                      <Document><Type>Invoice</Type><Id>1</Id></Document>
                      <Document><Type>CreditNote</Type><Id>2</Id></Document>
                  </ArrayOfDocument>
                  """;

        // When
        var actual = Deserialize(xml);

        // Then
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
        var xml = """
                  <ArrayOfDocument>
                      <Document><Type>Invoice</Type><Id>1</Id></Document>
                      <Document><Type>CreditNote</Type><Id>2</Id></Document>
                      <Document><Type>Order</Type><Id>3</Id></Document>
                  </ArrayOfDocument>
                  """;

        // When
        var actual = Deserialize(xml);

        // Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Document(DocumentType.Invoice, 1),
            new Document(DocumentType.CreditNote, 2)
        });
    }

    [Test]
    public void Deserialize_unknown_numeric_enum_value()
    {
        // Given
        var xml = """
                  <ArrayOfDocument>
                      <Document><Type>Invoice</Type><Id>1</Id></Document>
                      <Document><Type>CreditNote</Type><Id>2</Id></Document>
                      <Document><Type>3</Type><Id>3</Id></Document>
                  </ArrayOfDocument>
                  """;

        // When
        var actual = Deserialize(xml);

        // Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Document(DocumentType.Invoice, 1),
            new Document(DocumentType.CreditNote, 2)
        });
    }

    [Test]
    public void Deserialize_unknown_enum_value_client_fix()
    {
        // Given
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <ArrayOfDocument xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                      <Document><Type>Invoice</Type><Id>1</Id></Document>
                      <Document><Type>CreditNote</Type><Id>2</Id></Document>
                      <Document><Type>Order</Type><Id>3</Id></Document>
                  </ArrayOfDocument>
                  """;

        // When
        var serializer = new XmlSerializer(typeof(DocumentFixed[]));
        var xmlAsStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var actual = serializer.Deserialize(xmlAsStream);

        // Then
        actual.Should().BeEquivalentTo(new[]
        {
            new DocumentFixed(DocumentType.Invoice, 1),
            new DocumentFixed(DocumentType.CreditNote, 2),
            new DocumentFixed(DocumentType.Undefined, 3),
        });
    }

    [Test]
    public void PrintXml()
    {
        var serializer = new XmlSerializer(typeof(Document[]));
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter);
        serializer.Serialize(xmlWriter, new[]
        {
            new Document(DocumentType.Invoice, 1),
            new Document(DocumentType.CreditNote, 2)
        });
        Console.WriteLine(stringWriter.ToString());
    }

    private static Document[] Deserialize(string xml)
    {
        var serializer = new XmlSerializer(typeof(Document[]));
        var xmlAsStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return (Document[])serializer.Deserialize(xmlAsStream);
    }
}

public class Document
{
    public Document()
    {
    }

    public Document(DocumentType type, int id)
    {
        Type = type;
        Id = id;
    }

    public DocumentType Type { get; set; }
    public int Id { get; set; }
}

[XmlType(TypeName = "Document")]
public class DocumentFixed
{
    public DocumentFixed()
    {
    }

    public DocumentFixed(string type, int id)
    {
        Type = type;
        Id = id;
    }

    public DocumentFixed(DocumentType type, int id)
    {
        TypeAsEnum = type;
        Id = id;
    }

    [XmlIgnore] 
    public DocumentType TypeAsEnum { get; set; }
    public string Type
    {
        get => TypeAsEnum.ToString("G");
        set => TypeAsEnum = Enum.TryParse<DocumentType>(value, out var result) ? result : default;
    }

    public int Id { get; set; }
}
