using System;

namespace Gaev.Blog.Examples.PiiManagement;

public class User
{
    public Guid Id { get; set; }
    public PiiString Name { get; set; }
    public PiiString Email { get; set; }
    public PiiString IAmNull { get; set; }
}