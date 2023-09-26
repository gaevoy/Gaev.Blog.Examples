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
    public void It_should_deserialize_all_known_enum_values()
    {
        // Given
        var xml = """
                  <ArrayOfMoney>
                      <Money><Currency>EUR</Currency><Amount>1</Amount></Money>
                      <Money><Currency>USD</Currency><Amount>2</Amount></Money>
                  </ArrayOfMoney>
                  """;

        // When
        var actual = Deserialize(xml);

        // Then
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
        var xml = """
                  <ArrayOfMoney>
                      <Money><Currency>EUR</Currency><Amount>1</Amount></Money>
                      <Money><Currency>USD</Currency><Amount>2</Amount></Money>
                      <Money><Currency>Bitcoin</Currency><Amount>3</Amount></Money>
                  </ArrayOfMoney>
                  """;

        // When
        var actual = Deserialize(xml);

        // Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Money(Currency.EUR, 1),
            new Money(Currency.USD, 2)
        });
    }

    [Test]
    public void It_should_deserialize_unknown_numeric_enum_value()
    {
        // Given
        var xml = """
                  <ArrayOfMoney>
                      <Money><Currency>EUR</Currency><Amount>1</Amount></Money>
                      <Money><Currency>USD</Currency><Amount>2</Amount></Money>
                      <Money><Currency>3</Currency><Amount>3</Amount></Money>
                  </ArrayOfMoney>
                  """;

        // When
        var actual = Deserialize(xml);

        // Then
        actual.Should().BeEquivalentTo(new[]
        {
            new Money(Currency.EUR, 1),
            new Money(Currency.USD, 2)
        });
    }

    [Test]
    public void It_should_deserialize_unknown_enum_value_client_fix()
    {
        // Given
        var xml = """
                  <ArrayOfMoney>
                      <Money><Currency>EUR</Currency><Amount>1</Amount></Money>
                      <Money><Currency>USD</Currency><Amount>2</Amount></Money>
                      <Money><Currency>Bitcoin</Currency><Amount>3</Amount></Money>
                  </ArrayOfMoney>
                  """;

        // When
        var serializer = new XmlSerializer(typeof(MoneyFixed[]));
        var xmlAsStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var actual = serializer.Deserialize(xmlAsStream);

        // Then
        actual.Should().BeEquivalentTo(new[]
        {
            new MoneyFixed(Currency.EUR, 1),
            new MoneyFixed(Currency.USD, 2),
            new MoneyFixed(Currency.Undefined, 3),
        });
    }

    [Test]
    public void It_should_print_serialized_Xml()
    {
        var serializer = new XmlSerializer(typeof(Money[]));
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter);
        serializer.Serialize(xmlWriter, new[]
        {
            new Money(Currency.EUR, 1),
            new Money(Currency.USD, 2)
        });
        Console.WriteLine(stringWriter.ToString());
    }

    private static Money[] Deserialize(string xml)
    {
        var serializer = new XmlSerializer(typeof(Money[]));
        var xmlAsStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return (Money[])serializer.Deserialize(xmlAsStream);
    }
}

public class Money
{
    public Money()
    {
    }

    public Money(Currency currency, decimal amount)
    {
        Currency = currency;
        Amount = amount;
    }

    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
}

[XmlType(TypeName = "Money")]
public class MoneyFixed
{
    public MoneyFixed()
    {
    }

    public MoneyFixed(string currency, decimal amount)
    {
        Currency = currency;
        Amount = amount;
    }

    public MoneyFixed(Currency currency, decimal amount)
    {
        CurrencyAsEnum = currency;
        Amount = amount;
    }

    [XmlIgnore] 
    public Currency CurrencyAsEnum { get; set; }
    public string Currency
    {
        get => CurrencyAsEnum.ToString("G");
        set => CurrencyAsEnum = Enum.TryParse<Currency>(value, out var result) ? result : default;
    }

    public decimal Amount { get; set; }
}
