namespace Gaev.Blog.Examples.PiiManagement.PiiSerializers;

public class PlainText : IPiiSerializer
{
    public string ToString(PiiString piiString) => piiString.ToString();

    public PiiString FromString(string str) => new PiiString(str);
}