using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace Gaev.Blog;

public class CryptoUtilsTests
{
    public string ComputeSha256(MemoryStream stream)
    {
        using var sha = SHA256.Create();
        return string.Join("", sha.ComputeHash(stream).Select(b => b.ToString("x2")));
    }

    public string ComputeSha256Fixed(MemoryStream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using var sha = SHA256.Create();
        return string.Join("", sha.ComputeHash(stream).Select(b => b.ToString("x2")));
    }

    [TestCase("John Doe", "6cea57c2fb6cbc2a40411135005760f241fffc3e5e67ab99882726431037f908")]
    [TestCase("C# developer", "c9298659b4622ec5881c09fc510f23fcfbe75159d13f64b388b74c4d060d65d7")]
    public void It_should_compute_SHA256_for_stream(string payload, string expected)
    {
        // Given
        var binary = Encoding.UTF8.GetBytes(payload);
        var stream = new MemoryStream(binary);

        // When
        var actual = ComputeSha256(stream);

        // Then
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("John Doe", "6cea57c2fb6cbc2a40411135005760f241fffc3e5e67ab99882726431037f908")]
    [TestCase("C# developer", "c9298659b4622ec5881c09fc510f23fcfbe75159d13f64b388b74c4d060d65d7")]
    public void It_should_compute_SHA256_for_stream_broken(string payload, string expected)
    {
        // Given
        var binary = Encoding.UTF8.GetBytes(payload);
        var stream = new MemoryStream();
        stream.Write(binary);

        // When
        var actual = ComputeSha256(stream);

        // Then
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("John Doe", "6cea57c2fb6cbc2a40411135005760f241fffc3e5e67ab99882726431037f908")]
    [TestCase("C# developer", "c9298659b4622ec5881c09fc510f23fcfbe75159d13f64b388b74c4d060d65d7")]
    public void It_should_compute_SHA256_for_stream_fixed(string payload, string expected)
    {
        // Given
        var binary = Encoding.UTF8.GetBytes(payload);
        var stream = new MemoryStream();
        stream.Write(binary);

        // When
        var actual = ComputeSha256Fixed(stream);

        // Then
        Assert.That(actual, Is.EqualTo(expected));
    }
}
