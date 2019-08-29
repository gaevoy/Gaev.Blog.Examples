using System;

namespace Gaev.Blog.Examples
{
    public class Customer
    {
        public Customer(Guid id, DateTime createdAt, string name)
        {
            Id = id;
            CreatedAt = createdAt;
            Name = name;
        }

        public Guid Id { get; }
        public DateTime CreatedAt { get; }
        public string Name { get; }
    }
}