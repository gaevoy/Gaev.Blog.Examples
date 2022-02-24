using FluentAssertions;
using NUnit.Framework;

namespace Gaev.Blog.Examples;

public class PiiStringTests
{
    [TestCase("a", "a", true)]
    [TestCase("", "", true)]
    [TestCase(null, null, true)]
    [TestCase("a", "b", false)]
    [TestCase("a", null, false)]
    [TestCase(null, "b", false)]
    public void EqOperator(string a, string b, bool expected)
    {
        var actual = (PiiString)a == (PiiString)b;
        actual.Should().Be(expected);
    }

    [TestCase("a", "a", false)]
    [TestCase("", "", false)]
    [TestCase(null, null, false)]
    [TestCase("a", "b", true)]
    [TestCase("a", null, true)]
    [TestCase(null, "b", true)]
    public void NotEqOperator(string a, string b, bool expected)
    {
        var actual = (PiiString)a != (PiiString)b;
        actual.Should().Be(expected);
    }

    [TestCase("a", "a", true)]
    [TestCase("", "", true)]
    [TestCase("a", "b", false)]
    [TestCase("a", null, false)]
    public void Equals(string a, string b, bool expected)
    {
        var aAsPiiString = (PiiString)a;
        aAsPiiString.Equals((PiiString)b).Should().Be(expected);
        aAsPiiString.Equals((object)b).Should().Be(expected);
    }
}