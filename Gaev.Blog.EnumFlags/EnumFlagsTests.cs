using FluentAssertions;
using NUnit.Framework;

namespace Gaev.Blog.EnumFlags;

#if DEBUG
public class EnumFlagsTests
{
    [Test]
    public void It_should_raise_enum_flag()
    {
        // Given
        var preferences = Pet.Cat;

        // When
        preferences = preferences | Pet.Dog;

        // Then
        preferences.Should().Be(Pet.Cat | Pet.Dog);
        preferences.HasFlag(Pet.Dog).Should().Be(true);
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(false);

        // Explanation
        //    0010 - Cat
        // OR
        //    0001 - Dog
        //    ----
        //    0011 - Cat with Dog
    }

    [Test]
    public void It_should_lower_enum_flag()
    {
        // Given
        var preferences = Pet.Cat | Pet.Bird;

        // When
        preferences = preferences & ~Pet.Bird;

        // Then
        preferences.Should().Be(Pet.Cat);
        preferences.HasFlag(Pet.Dog).Should().Be(false);
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(false);

        // Explanation
        //     0110 - Cat with Bird
        // AND
        //     1011 - NOT(Bird) which is NOT(0100)
        //     ----
        //     0010 - Cat
    }

    [Test]
    public void It_should_raise_enum_flag_via_SetFlag()
    {
        // Given
        var preferences = Pet.Cat;

        // When
        preferences = preferences.SetFlag(Pet.Dog, true);

        // Then
        preferences.Should().Be(Pet.Cat | Pet.Dog);
        preferences.HasFlag(Pet.Dog).Should().Be(true);
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(false);
    }

    [Test]
    public void It_should_lower_enum_flag_via_SetFlag()
    {
        // Given
        var preferences = Pet.Cat | Pet.Bird;

        // When
        preferences = preferences.SetFlag(Pet.Bird, false);

        // Then
        preferences.Should().Be(Pet.Cat);
        preferences.HasFlag(Pet.Dog).Should().Be(false);
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(false);
    }

    [Test]
    public void It_should_raise_enum_flag_via_RaiseFlag()
    {
        // Given
        var preferences = Pet.Cat;

        // When
        preferences = preferences.RaiseFlag(Pet.Dog);

        // Then
        preferences.Should().Be(Pet.Cat | Pet.Dog);
        preferences.HasFlag(Pet.Dog).Should().Be(true);
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(false);
    }

    [Test]
    public void It_should_lower_enum_flag_via_LowerFlag()
    {
        // Given
        var preferences = Pet.Cat | Pet.Bird;

        // When
        preferences = preferences.LowerFlag(Pet.Bird);

        // Then
        preferences.Should().Be(Pet.Cat);
        preferences.HasFlag(Pet.Dog).Should().Be(false);
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(false);
    }
}
#endif

// @formatter:off
[Flags]
public enum Pet
{
    None =   0b_0000,
    Dog =    0b_0001,
    Cat =    0b_0010,
    Bird =   0b_0100,
    Rabbit = 0b_1000
}
// @formatter:no
