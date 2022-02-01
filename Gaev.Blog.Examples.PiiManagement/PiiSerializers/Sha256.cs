using System;
using System.Security.Cryptography;
using System.Text;

namespace Gaev.Blog.Examples.PiiManagement.PiiSerializers;

public class Sha256 : IPiiSerializer
{
    public string ToString(PiiString piiString)
    {
        var dataToHash = Encoding.UTF8.GetBytes(piiString.ToString());
        using var sha = SHA256.Create();
        var hashedBuffer = sha.ComputeHash(dataToHash);
        return Convert.ToBase64String(hashedBuffer);
    }

    public PiiString FromString(string str)
    {
        throw new NotSupportedException();
    }
}