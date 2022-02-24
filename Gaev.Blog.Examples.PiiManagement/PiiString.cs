using System;

namespace Gaev.Blog.Examples;

public class PiiString
{
    private readonly string _string;

    public PiiString(string underlyingString)
        => _string = underlyingString ?? throw new ArgumentNullException(nameof(underlyingString));

    public override string ToString()
        => _string;

    public override int GetHashCode()
        => _string.GetHashCode();

    public override bool Equals(object obj)
        => obj switch
        {
            PiiString other => AreEqual(this, other),
            string other => AreEqual(this, new PiiString(other)),
            _ => false
        };

    public static bool operator ==(PiiString a, PiiString b)
        => AreEqual(a, b);

    public static bool operator !=(PiiString a, PiiString b)
        => !AreEqual(a, b);

    public static implicit operator string(PiiString piiString)
        => piiString?._string;

    public static implicit operator PiiString(string underlyingString)
        => underlyingString == null ? null : new PiiString(underlyingString);

    private static bool AreEqual(PiiString a, PiiString b) =>
        (a, b) switch
        {
            (null, null) => true,
            (null, _) => false,
            (_, null) => false,
            (_, _) => a._string.Equals(b._string)
        };
}