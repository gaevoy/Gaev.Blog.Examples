using System;

namespace Gaev.Blog.Examples
{
    public class CustomerService : ICustomerService
    {
        public Customer RegisterCustomer(string name)
        {
            return new Customer(NewId(), GetUtcNow(), name);
        }

        private Guid NewId() => Guid.NewGuid();
        private DateTime GetUtcNow() => DateTime.UtcNow;
    }
}