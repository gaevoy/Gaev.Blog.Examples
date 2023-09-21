// ReSharper disable InconsistentNaming

namespace Gaev.Blog.EnumAsStringTrap;

public record Money(Currency Currency, decimal Amount);

public enum Currency
{
    Undefined = 0,
    EUR = 1,
    USD = 2
}
