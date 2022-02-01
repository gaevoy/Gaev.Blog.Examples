using System;
using System.Security.Cryptography;
using System.Text;

namespace Gaev.Blog.Examples.PiiManagement.PiiSerializers;

public class Aes128 : IPiiSerializer
{
    private readonly string _key;
    private const string Iv = "DDRKFPeWVwkvBm1rw3OHrA==";

    public Aes128(string key)
    {
        _key = key;
    }

    public string ToString(PiiString piiString)
    {
        var stringToEncrypt = piiString.ToString();
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_key);
        aes.IV = Convert.FromBase64String(Iv);
        using var encryptor = aes.CreateEncryptor();
        var buffer = Encoding.UTF8.GetBytes(stringToEncrypt);
        var encryptedBuffer = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
        return Convert.ToBase64String(encryptedBuffer);
    }

    public PiiString FromString(string str)
    {
        var dataToDecrypt = Convert.FromBase64String(str);
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_key);
        aes.IV = Convert.FromBase64String(Iv);
        using var decryptor = aes.CreateDecryptor();
        var decryptedBuffer = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
        return new PiiString(Encoding.UTF8.GetString(decryptedBuffer));
    }
}