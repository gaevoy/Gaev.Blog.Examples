using System;

namespace Gaev.Blog.Examples
{
    public class CustomerServiceViaInterface: ICustomerService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CustomerServiceViaInterface(IGuidGenerator guidGenerator, IDateTimeProvider dateTimeProvider)
        {
            _guidGenerator = guidGenerator;
            _dateTimeProvider = dateTimeProvider;
        }

        public Customer RegisterCustomer(string name)
        {
            return new Customer(_guidGenerator.NewGuid(), _dateTimeProvider.GetUtcNow(), name);
        }
    }

    public interface IGuidGenerator
    {
        Guid NewGuid();
    }

    public class GuidGenerator : IGuidGenerator
    {
        public Guid NewGuid() => Guid.NewGuid();
    }

    public interface IDateTimeProvider
    {
        DateTime GetUtcNow();
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}