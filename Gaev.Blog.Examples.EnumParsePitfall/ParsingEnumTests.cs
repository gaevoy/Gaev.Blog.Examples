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
        public void It_should_parse_VAT_number(string vatNumber, CountryCode expected)
        {
            // Given
            string twoLetterCode = vatNumber[..2];

            // When
            CountryCode actual = ParseFixed(twoLetterCode);

            // Then
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, Repeat(100)]
        public void It_should_parse_VAT_number_and_return_undefined()
        {
            // Given
            string randomVatNumber = TestContext.CurrentContext.Random.Next(1000000, 9999999).ToString();
            string twoLetterCode = randomVatNumber[..2];

            // When
            CountryCode countryCode = ParseFixed(twoLetterCode);

            // Then
            Assert.That(countryCode, Is.EqualTo(CountryCode.Undefined));
        }

        private static CountryCode ParseBroken1(string twoLetterCode)
        {
            if (Enum.TryParse(twoLetterCode, out CountryCode countryCode))
                return countryCode;

            return CountryCode.Undefined;
        }

        private static CountryCode ParseBroken2(string twoLetterCode)
        {
            if (Enum.TryParse(twoLetterCode, out CountryCode countryCode))
                if (Enum.IsDefined(countryCode))
                    return countryCode;

            return CountryCode.Undefined;
        }

        private static CountryCode ParseBrokenViaJson(string twoLetterCode)
        {
            var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
            var json = JsonSerializer.Serialize(twoLetterCode);
            return JsonSerializer.Deserialize<CountryCode>(json, options);
        }

        private static CountryCode ParseFixed(string twoLetterCode)
        {
            return Enum
                .GetValues<CountryCode>()
                .FirstOrDefault(val => Enum.GetName(val) == twoLetterCode);
        }

        private static CountryCode ParseFixedMemoryFriendly(string twoLetterCode)
        {
            if (EnumCache<CountryCode>.ValueByName.TryGetValue(twoLetterCode, out var countryCode))
                return countryCode;

            return CountryCode.Undefined;
        }
    }

    public class EnumCache<TEnum> where TEnum : struct, Enum
    {
        public static readonly Dictionary<string, TEnum> ValueByName
            = Enum
                .GetValues<TEnum>()
                .ToDictionary(val => Enum.GetName(val), val => val);
    }
}