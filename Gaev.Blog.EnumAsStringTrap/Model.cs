namespace Gaev.Blog.EnumAsStringTrap;

public record Document(DocumentType Type, int Id);

public enum DocumentType
{
    Undefined = 0,
    Invoice = 1,
    CreditNote = 2
}
