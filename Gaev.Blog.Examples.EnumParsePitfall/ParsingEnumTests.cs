using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Gaev.Blog.Examples
{
    public enum CountryCode
    {
        Undefined = default,
        DK = 10,
        PL = 20
    }

    public class ParsingEnumTests
    {
        [TestCase("DK:1034567", CountryCode.DK)]
        [TestCase("PL:1034567", CountryCode.PL)]
        [TestCase("1034567", CountryCode.Undefined)]
        public void It_should_parse(string vatNumber, CountryCode expected)
        {
            // Given
            string twoLetterCode = vatNumber[..2];

            // When
            CountryCode actual = Parse(twoLetterCode);

            // Then
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Repeat(100)]
        public void It_should_parse_as_undefined()
        {
            // Given
            var randomizer = TestContext.CurrentContext.Random;
            string randomVatNumber = randomizer.Next(1000000, 9999999).ToString();
            string twoLetterCode = randomVatNumber[..2];

            // When
            CountryCode countryCode = Parse(twoLetterCode);

            // Then
            Assert.That(countryCode, Is.EqualTo(CountryCode.Undefined));
        }

        public static CountryCode ParseBroken1(string twoLetterCode)
        {
            if (Enum.TryParse(twoLetterCode, out CountryCode countryCode))
                return countryCode;

            return CountryCode.Undefined;
        }

        public static CountryCode ParseBroken2(string twoLetterCode)
        {
            if (Enum.TryParse(twoLetterCode, out CountryCode countryCode))
                if (Enum.IsDefined(countryCode))
                    return countryCode;

            return CountryCode.Undefined;
        }

        public static CountryCode ParseBrokenViaJson(string twoLetterCode)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize(twoLetterCode);
            return JsonSerializer.Deserialize<CountryCode>(json, options);
        }

        public static CountryCode Parse(string twoLetterCode)
        {
            return Enum
                .GetValues<CountryCode>()
                .FirstOrDefault(val => Enum.GetName(val) == twoLetterCode);
        }

        public static CountryCode ParseFixedMemoryFriendly(string twoLetterCode)
        {
            var valueByName = EnumCache<CountryCode>.ValueByName;
            if (valueByName.TryGetValue(twoLetterCode, out var countryCode))
                return countryCode;

            return CountryCode.Undefined;
        }
    }

    public class EnumCache<TEnum> where TEnum : struct, Enum
    {
        public static readonly Dictionary<string, TEnum> ValueByName
            = Enum.GetValues<TEnum>().ToDictionary(Enum.GetName);
    }
}