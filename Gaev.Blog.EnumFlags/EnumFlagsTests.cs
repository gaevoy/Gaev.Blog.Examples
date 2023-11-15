using System.Runtime.CompilerServices;
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
        preferences = preferences | Pet.Rabbit;

        // Then
        preferences.Should().Be(Pet.Cat | Pet.Rabbit);
        preferences.HasFlag(Pet.Dog).Should().Be(false);
        preferences.HasFlag(Pet.Cat).Should().Be(true);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(true);

        // Explanation
        //    0010 - Cat
        // OR
        //    1000 - Rabbit
        //    ----
        //    1010 - Cat with Rabbit
    }

    [Test]
    public void It_should_lower_enum_flag()
    {
        // Given
        var preferences = Pet.Cat | Pet.Rabbit;

        // When
        preferences = preferences & ~Pet.Cat;

        // Then
        preferences.Should().Be(Pet.Rabbit);
        preferences.HasFlag(Pet.Dog).Should().Be(false);
        preferences.HasFlag(Pet.Cat).Should().Be(false);
        preferences.HasFlag(Pet.Bird).Should().Be(false);
        preferences.HasFlag(Pet.Rabbit).Should().Be(true);

        // Explanation
        //     1010 - Cat with Rabbit
        // AND
        //     1101 - NOT(Cat) which is NOT(0010)
        //     ----
        //     1000 - Rabbit
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

    [TestCase(Pet.Cat | Pet.Dog, Pet.Bird, true, Pet.Cat | Pet.Dog | Pet.Bird)]
    [TestCase(Pet.Cat | Pet.Dog, Pet.Dog, true, Pet.Cat | Pet.Dog)]
    [TestCase(Pet.Cat | Pet.Dog, Pet.Cat, false, Pet.Dog)]
    [TestCase(Pet.Cat | Pet.Dog, Pet.Bird, false, Pet.Cat | Pet.Dog)]
    public void It_should_set_flag_via_ugly_code(Pet initial, Pet flag, bool state, Pet expected)
    {
        // Given
        var preferences = initial;

        // When
        // Based on: https://github.com/dotnet/runtime/issues/14084#issuecomment-803638941
        var stateMask = (Pet)(-1 * Convert.ToInt32(state)); // same as `state ? -1 : 0`
        preferences = (preferences & ~flag) | (stateMask & flag);

        // Then
        preferences.Should().Be(expected);
        /* JavaScript version: https://jsfiddle.net/5h6j04a9/
const dog =  0b0001;
const cat =  0b0010;
const bird = 0b0100;
let myPets = cat | dog;

// raise
let state = true;
let flag = bird;
myPets = (myPets & ~flag) | (-state & flag);

// lower
state = false;
flag = cat;
myPets = (myPets & ~flag) | (-state & flag);
        */
    }
}
#endif

// @formatter:off
[Flags]
public enum Pet
{
    Dog =    0b_0001, // 1 - the loyal friend
    Cat =    0b_0010, // 2 - the whiskered boss
    Bird =   0b_0100, // 4 - the chirpy companion
    Rabbit = 0b_1000  // 8 - the fluffy hopper
}
// @formatter:no
