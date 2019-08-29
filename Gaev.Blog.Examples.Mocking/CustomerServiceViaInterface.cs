using System;

namespace Gaev.Blog.Examples
{
    public class CustomerServiceViaInterface: ICustomerService
    {
        private readonly IIdGenerator _idGenerator;
        private readonly ISystemTime _systemTime;

        public CustomerServiceViaInterface(IIdGenerator idGenerator, ISystemTime systemTime)
        {
            _idGenerator = idGenerator;
            _systemTime = systemTime;
        }

        public Customer RegisterCustomer(string name)
        {
            return new Customer(_idGenerator.NewId(), _systemTime.GetUtcNow(), name);
        }
    }

    public interface IIdGenerator
    {
        Guid NewId();
    }

    public class IdGenerator : IIdGenerator
    {
        public Guid NewId() => Guid.NewGuid();
    }

    public interface ISystemTime
    {
        DateTime GetUtcNow();
    }

    public class SystemTime : ISystemTime
    {
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}