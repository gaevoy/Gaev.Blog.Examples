using System;
using System.Linq;
using Autofac;
using NSubstitute;
using NUnit.Framework;

namespace Gaev.Blog.Examples
{
    public class DelegateVsInterfaceTests
    {
        #region CustomerServiceViaDelegate

        [Test]
        public void CustomerServiceViaDelegate_should_register_customer()
        {
            // Given
            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var name = Guid.NewGuid().ToString();
            var service = new CustomerServiceViaDelegate(newId: () => id, getUtcNow: () => now);

            // When
            var customer = service.RegisterCustomer(name);

            // Then
            Assert.That(customer.Id, Is.EqualTo(id));
            Assert.That(customer.CreatedAt, Is.EqualTo(now));
            Assert.That(customer.Name, Is.EqualTo(name));
        }

        [Test]
        public void AutoFac_should_resolve_CustomerServiceViaDelegate()
        {
            // Given
            var builder = new ContainerBuilder();
            builder.RegisterType<CustomerServiceViaDelegate>().As<ICustomerService>();
            var container = builder.Build();

            // When
            var service = container.Resolve<ICustomerService>();

            // Then
            Assert.That(service, Is.TypeOf<CustomerServiceViaDelegate>());
        }

        [Test]
        public void CustomerServiceViaDelegate_should_generate_unique_IDs()
        {
            // Given
            var service = new CustomerServiceViaDelegate();

            // When
            var ids = Enumerable.Range(0, 100).Select(_ => service.NewId()).ToList();

            // Then
            Assert.That(ids, Is.Unique);
        }

        [Test]
        public void CustomerServiceViaDelegate_should_get_current_UTC_date()
        {
            // Given
            var service = new CustomerServiceViaDelegate();

            // When
            var now = service.GetUtcNow();

            // Then
            Assert.That(now, Is.EqualTo(DateTime.UtcNow).Within(50).Milliseconds);
        }

        #endregion

        #region CustomerServiceViaInterface

        [Test]
        public void CustomerServiceViaInterface_should_register_customer()
        {
            // Given
            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var name = Guid.NewGuid().ToString();
            var idGenerator = Substitute.For<IIdGenerator>();
            idGenerator.NewId().Returns(id);
            var systemTime = Substitute.For<ISystemTime>();
            systemTime.GetUtcNow().Returns(now);
            var service = new CustomerServiceViaInterface(idGenerator, systemTime);

            // When
            var customer = service.RegisterCustomer(name);

            // Then
            Assert.That(customer.Id, Is.EqualTo(id));
            Assert.That(customer.CreatedAt, Is.EqualTo(now));
            Assert.That(customer.Name, Is.EqualTo(name));
        }

        [Test]
        public void AutoFac_should_resolve_CustomerServiceViaInterface()
        {
            // Given
            var builder = new ContainerBuilder();
            builder.RegisterType<IdGenerator>().As<IIdGenerator>();
            builder.RegisterType<SystemTime>().As<ISystemTime>();
            builder.RegisterType<CustomerServiceViaInterface>().As<ICustomerService>();
            var container = builder.Build();

            // When
            var service = container.Resolve<ICustomerService>();

            // Then
            Assert.That(service, Is.TypeOf<CustomerServiceViaInterface>());
        }

        [Test]
        public void IdGenerator_should_generate_unique_IDs()
        {
            // Given
            IIdGenerator idGenerator = new IdGenerator();

            // When
            var ids = Enumerable.Range(0, 100).Select(_ => idGenerator.NewId()).ToList();

            // Then
            Assert.That(ids, Is.Unique);
        }

        [Test]
        public void SystemTime_should_get_current_UTC_date()
        {
            // Given
            ISystemTime systemTime = new SystemTime();

            // When
            var now = systemTime.GetUtcNow();

            // Then
            Assert.That(now, Is.EqualTo(DateTime.UtcNow).Within(50).Milliseconds);
        }

        #endregion
    }
}