namespace Gaev.Blog.Examples;

public interface IPiiEncoder
{
    string ToSystemString(PiiString piiString);
    PiiString ToPiiString(string str);
}