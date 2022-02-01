using System;

namespace Gaev.Blog.Examples.PiiManagement;

public class PiiString
{
    private readonly string _string;

    public PiiString(string underlyingString)
        => _string = underlyingString ?? throw new ArgumentNullException(nameof(underlyingString));

    public static implicit operator string(PiiString piiString)
        => piiString?._string;

    public static implicit operator PiiString(string underlyingString)
        => underlyingString == null ? null : new PiiString(underlyingString);

    public override string ToString()
        => _string;

    public override int GetHashCode()
        => _string.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj is PiiString piiString)
            return _string.Equals(piiString._string);
        return _string.Equals(obj);
    }
}