using System;
using NUnit.Framework;

namespace Gaev.Blog.Examples
{
    public class OrderOfEnumValuesMatters
    {
        [Test]
        public void It_should_treat_a_black_as_is()
        {
            Assert.AreEqual("Black", ((Colors_v1) 4).ToString());
            Assert.AreEqual("Black", Enum.Parse<Colors_v1>("Black").ToString());
        }

        [Test]
        public void It_should_use_a_white_instead_of_a_black_FAULTY()
        {
            Assert.AreEqual("White", ((Colors_v2) 4).ToString());
            Assert.AreEqual("White", Enum.Parse<Colors_v2>("Black").ToString());
        }

        [Test]
        public void It_should_use_a_white_instead_of_a_black_FIXED()
        {
            Assert.AreEqual("White", ((Colors_v3) 4).ToString());
            Assert.AreEqual("White", Enum.Parse<Colors_v3>("Black").ToString());
        }
    }

    public enum Colors_v1
    {
        Red = 1,
        Green = 2,
        Blue = 3,
        Black = 4
    }

    public enum Colors_v2
    {
        Red = 1,
        Green = 2,
        Blue = 3,
        [Obsolete] Black = White,
        White = 4
    }

    public enum Colors_v3
    {
        Red = 1,
        Green = 2,
        Blue = 3,
        White = 4,
        [Obsolete] Black = White
    }
}