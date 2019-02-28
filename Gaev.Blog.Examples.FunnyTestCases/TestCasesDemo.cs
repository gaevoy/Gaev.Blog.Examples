using NUnit.Framework;

// ReSharper disable MemberCanBePrivate.Local
#pragma warning disable 649

namespace Gaev.Blog.Examples
{
    [TestFixture]
    public class TestCasesDemo
    {
        class User
        {
            public string FirstName;
            public string LastName;
            public string JobTitle;
            public string AboutYourself;
            public string ResidenceCity;

            public bool IsValid => FirstName != null
                                   && LastName != null
                                   && JobTitle != null
                                   && AboutYourself != null
                                   && ResidenceCity != null;
        }

        [TestCase(true, true, true, true, true, true)]
        [TestCase(true, false, false, false, false, false)]
        [TestCase(false, true, false, false, false, false)]
        [TestCase(false, false, true, false, false, false)]
        [TestCase(false, false, false, true, false, false)]
        [TestCase(false, false, false, false, true, false)]
        [TestCase(false, false, false, false, false, false)]
        public void User_should_be_valid_0(
            bool hasFirstName,
            bool hasLastName,
            bool hasJobTitle,
            bool hasAboutYourself,
            bool hasResidenceCity,
            bool isValid
        )
        {
            // Given
            var user = new User
            {
                FirstName = hasFirstName ? "John" : null,
                LastName = hasLastName ? "Doe" : null,
                JobTitle = hasJobTitle ? ".NET developer" : null,
                AboutYourself = hasAboutYourself ? "Dreamed of being a cowboy but became a developer" : null,
                ResidenceCity = hasResidenceCity ? "Krakow, Poland" : null
            };

            // When
            var actual = user.IsValid;

            // Then
            Assert.That(actual, Is.EqualTo(isValid));
        }

        [Test, Sequential]
        public void User_should_be_valid_1(
            [Values(true, true, false, false, false, false, false)] bool hasFirstName,
            [Values(true, false, true, false, false, false, false)] bool hasLastName,
            [Values(true, false, false, true, false, false, false)] bool hasJobTitle,
            [Values(true, false, false, false, true, false, false)] bool hasAboutYourself,
            [Values(true, false, false, false, false, true, false)] bool hasResidenceCity,
            [Values(true, false, false, false, false, false, false)] bool isValid
        )
        {
            // Given
            var user = new User
            {
                FirstName = hasFirstName ? "John" : null,
                LastName = hasLastName ? "Doe" : null,
                JobTitle = hasJobTitle ? ".NET developer" : null,
                AboutYourself = hasAboutYourself ? "Dreamed of being a cowboy but became a developer" : null,
                ResidenceCity = hasResidenceCity ? "Krakow, Poland" : null
            };

            // When
            var actual = user.IsValid;

            // Then
            Assert.That(actual, Is.EqualTo(isValid));
        }


        const bool x = true;
        const bool _ = false;

        [Test, Sequential]
        public void User_should_be_valid_2(
            [Values(x, x, _, _, _, _, _)] bool hasFirstName,
            [Values(x, _, x, _, _, _, _)] bool hasLastName,
            [Values(x, _, _, x, _, _, _)] bool hasJobTitle,
            [Values(x, _, _, _, x, _, _)] bool hasAboutYourself,
            [Values(x, _, _, _, _, x, _)] bool hasResidenceCity,
            [Values(x, _, _, _, _, _, _)] bool isValid
        )
        {
            // Given
            var user = new User
            {
                FirstName = hasFirstName ? "John" : null,
                LastName = hasLastName ? "Doe" : null,
                JobTitle = hasJobTitle ? ".NET developer" : null,
                AboutYourself = hasAboutYourself ? "Dreamed of being a cowboy but became a developer" : null,
                ResidenceCity = hasResidenceCity ? "Krakow, Poland" : null
            };

            // When
            var actual = user.IsValid;

            // Then
            Assert.That(actual, Is.EqualTo(isValid));
        }

        const string __ = null;

        // @formatter:off
        [TestCase("John", "Doe", ".NET developer", "I'm a developer", "Krakow", x)]
        [TestCase(__,     "Doe", ".NET developer", "I'm a developer", "Krakow", _)]
        [TestCase("John", __,    ".NET developer", "I'm a developer", "Krakow", _)]
        [TestCase("John", "Doe", __,               "I'm a developer", "Krakow", _)]
        [TestCase("John", "Doe", ".NET developer", __,                "Krakow", _)]
        [TestCase("John", "Doe", ".NET developer", "I'm a developer", __,       _)]
        [TestCase(__,     __,    __,               __,                __,       _)]
        // @formatter:on
        public void User_should_be_valid_3(
            string firstName,
            string lastName,
            string jobTitle,
            string about,
            string city,
            bool isValid)
        {
            // Given
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                JobTitle = jobTitle,
                AboutYourself = about,
                ResidenceCity = city
            };

            // When
            var actual = user.IsValid;

            // Then
            Assert.That(actual, Is.EqualTo(isValid));
        }

        const bool I = true;
        const bool o = true;
        const bool A = true;
        const bool J = false;

        [Test, Sequential]
        public void User_should_be_valid_4(
            [Values(x, _, x, x, x, _, _)] bool hasFirstName,
            [Values(x, I, _, _, _, I, _)] bool hasLastName,
            [Values(x, I, o, _, o, I, _)] bool hasJobTitle,
            [Values(x, I, _, A, _, I, _)] bool hasAboutYourself,
            [Values(x, I, _, _, _, I, _)] bool hasResidenceCity,
            [Values(x, _, _, J, _, _, _)] bool isValid
        )
        {
            // Given
            var user = new User
            {
                FirstName = hasFirstName ? "John" : null,
                LastName = hasLastName ? "Doe" : null,
                JobTitle = hasJobTitle ? ".NET developer" : null,
                AboutYourself = hasAboutYourself ? "Dreamed of being a cowboy but became a developer" : null,
                ResidenceCity = hasResidenceCity ? "Krakow, Poland" : null
            };

            // When
            var actual = user.IsValid;

            // Then
            Assert.That(actual, Is.EqualTo(isValid));
        }
    }
}