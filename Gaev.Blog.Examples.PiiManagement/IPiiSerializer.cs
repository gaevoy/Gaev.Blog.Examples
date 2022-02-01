namespace Gaev.Blog.Examples.PiiManagement;

public interface IPiiSerializer
{
    string ToString(PiiString piiString);
    PiiString FromString(string str);
}