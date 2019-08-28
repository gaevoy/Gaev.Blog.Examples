using System;

namespace Gaev.Blog.Examples
{
    public class CustomerServiceViaDelegate : ICustomerService
    {
        public Func<Guid> NewGuid = () => Guid.NewGuid();
        public Func<DateTime> GetUtcNow = () => DateTime.UtcNow;

        public CustomerServiceViaDelegate(Func<Guid> newGuid = null, Func<DateTime> getUtcNow = null)
        {
            NewGuid = newGuid ?? NewGuid;
            GetUtcNow = getUtcNow ?? GetUtcNow;
        }

        public Customer RegisterCustomer(string name)
        {
            return new Customer(NewGuid(), GetUtcNow(), name);
        }
    }
}