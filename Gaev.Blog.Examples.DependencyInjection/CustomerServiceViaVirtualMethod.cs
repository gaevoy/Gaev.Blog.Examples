using System;

namespace Gaev.Blog.Examples
{
    public class CustomerServiceViaVirtualMethod : ICustomerService
    {
        public Customer RegisterCustomer(string name)
        {
            return new Customer(NewId(), GetUtcNow(), name);
        }

        public virtual Guid NewId() => Guid.NewGuid();
        public virtual DateTime GetUtcNow() => DateTime.UtcNow;
    }
}