using System;

namespace Gaev.Blog.CSharp12AndNetFramework;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        ReadOnlySpan<int> numbers = [1, 2, 3, 4, 5, 6];
        // Index
        int secondToLast = numbers[^2];
        // Range
        var firstFour = numbers[..4];
        // List pattern
        if (numbers is [_, 2, var third, .. var rest])
        {
        }
    }

    // Records
    public record Person(string FirstName, string LastName);

    // Required member, init
    public class Config
    {
        public required string ConnectionString { get; init; }
    }
}
