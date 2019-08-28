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
            var service = new CustomerServiceViaDelegate(newGuid: () => id, getUtcNow: () => now);

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
        public void CustomerServiceViaDelegate_should_generate_unique_guid()
        {
            // Given
            var service = new CustomerServiceViaDelegate();

            // When
            var ids = Enumerable.Range(0, 100).Select(_ => service.NewGuid()).ToList();

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
            var guidGenerator = Substitute.For<IGuidGenerator>();
            guidGenerator.NewGuid().Returns(id);
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.GetUtcNow().Returns(now);
            var service = new CustomerServiceViaInterface(guidGenerator, dateTimeProvider);

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
            builder.RegisterType<GuidGenerator>().As<IGuidGenerator>();
            builder.RegisterType<DateTimeProvider>().As<IDateTimeProvider>();
            builder.RegisterType<CustomerServiceViaInterface>().As<ICustomerService>();
            var container = builder.Build();

            // When
            var service = container.Resolve<ICustomerService>();

            // Then
            Assert.That(service, Is.TypeOf<CustomerServiceViaInterface>());
        }
        
        [Test]
        public void GuidGenerator_should_generate_unique_guid()
        {
            // Given
            IGuidGenerator guidGenerator = new GuidGenerator();

            // When
            var ids = Enumerable.Range(0, 100).Select(_ => guidGenerator.NewGuid()).ToList();

            // Then
            Assert.That(ids, Is.Unique);
        }

        [Test]
        public void DateTimeProvider_should_get_current_UTC_date()
        {
            // Given
            IDateTimeProvider dateTimeProvider = new DateTimeProvider();

            // When
            var now = dateTimeProvider.GetUtcNow();

            // Then
            Assert.That(now, Is.EqualTo(DateTime.UtcNow).Within(50).Milliseconds);
        }

        #endregion
    }
}