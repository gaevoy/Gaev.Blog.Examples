namespace Gaev.Blog.Examples;

public class PiiAsPlainText : IPiiEncoder
{
    public string ToSystemString(PiiString piiString) => piiString.ToString();

    public PiiString ToPiiString(string str) => new PiiString(str);
}