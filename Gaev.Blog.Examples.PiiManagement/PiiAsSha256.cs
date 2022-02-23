using System;
using System.Security.Cryptography;
using System.Text;

namespace Gaev.Blog.Examples;

public class PiiAsSha256 : IPiiEncoder
{
    public string ToSystemString(PiiString piiString)
    {
        var dataToHash = Encoding.UTF8.GetBytes(piiString.ToString());
        using var sha = SHA256.Create();
        var hashedBuffer = sha.ComputeHash(dataToHash);
        return Convert.ToBase64String(hashedBuffer);
    }

    public PiiString ToPiiString(string str) => throw new NotSupportedException();
}