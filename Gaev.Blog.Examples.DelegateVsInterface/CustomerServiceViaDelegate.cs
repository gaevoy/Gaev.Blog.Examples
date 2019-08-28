using System;

namespace Gaev.Blog.Examples
{
    public class CustomerServiceViaDelegate : ICustomerService
    {
        public Func<Guid> NewId = () => Guid.NewGuid();
        public Func<DateTime> GetUtcNow = () => DateTime.UtcNow;

        public CustomerServiceViaDelegate(Func<Guid> newId = null, Func<DateTime> getUtcNow = null)
        {
            NewId = newId ?? NewId;
            GetUtcNow = getUtcNow ?? GetUtcNow;
        }

        public Customer RegisterCustomer(string name)
        {
            return new Customer(NewId(), GetUtcNow(), name);
        }
    }
}