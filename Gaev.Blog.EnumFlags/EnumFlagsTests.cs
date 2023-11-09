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
        var preferences = Pet.Cat; // 0010

        // When
        preferences = preferences | Pet.Dog; // 0010 | 0001 = 0011

        // Then
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Dog).Should().Be(true);
    }

    [Test]
    public void It_should_lower_enum_flag()
    {
        // Given
        var preferences = Pet.Cat | Pet.Bird; // 0110

        // When
        preferences = preferences & ~Pet.Bird; // 0110 & 1011 = 0010

        // Then
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
    }

    [Test]
    public void It_should_raise_enum_flag_via_SetFlag()
    {
        // Given
        var preferences = Pet.Cat;

        // When
        preferences = preferences.SetFlag(Pet.Dog, true);

        // Then
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Dog).Should().Be(true);
    }

    [Test]
    public void It_should_lower_enum_flag_via_SetFlag()
    {
        // Given
        var preferences = Pet.Cat | Pet.Bird;

        // When
        preferences = preferences.SetFlag(Pet.Bird, false);

        // Then
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
    }

    [Test]
    public void It_should_raise_enum_flag_via_RaiseFlag()
    {
        // Given
        var preferences = Pet.Cat;

        // When
        preferences = preferences.RaiseFlag(Pet.Dog);

        // Then
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Dog).Should().Be(true);
    }

    [Test]
    public void It_should_lower_enum_flag_via_LowerFlag()
    {
        // Given
        var preferences = Pet.Cat | Pet.Bird;

        // When
        preferences = preferences.LowerFlag(Pet.Bird);

        // Then
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
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
