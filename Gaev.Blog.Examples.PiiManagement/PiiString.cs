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
        => obj is PiiString piiString ? _string.Equals(piiString._string) : _string.Equals(obj);

    public static bool operator ==(PiiString a, PiiString b)
        => a?.Equals(b) == true;

    public static bool operator !=(PiiString a, PiiString b)
        => !(a == b);

    public static implicit operator string(PiiString piiString)
        => piiString?._string;

    public static implicit operator PiiString(string underlyingString)
        => underlyingString == null ? null : new PiiString(underlyingString);
}