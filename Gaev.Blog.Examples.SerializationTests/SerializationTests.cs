using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Gaev.Blog.Examples.Messages;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Gaev.Blog.Examples
{
    public class SerializationTests
    {
        [Test]
        public void It_should_serialize_then_deserialize_UserRegistered()
        {
            // Given
            var random = TestContext.CurrentContext.Random;
            var givenMessage = new UserRegistered(
                id: random.NextGuid(),
                email: random.GetString(),
                name: random.GetString());

            // When
            var json = JsonConvert.SerializeObject(givenMessage);
            var deserializedMessage = JsonConvert.DeserializeObject<UserRegistered>(json);

            // Then
            Assert.That(deserializedMessage.Id, Is.EqualTo(givenMessage.Id));
            Assert.That(deserializedMessage.Email, Is.EqualTo(givenMessage.Email));
            Assert.That(deserializedMessage.Name, Is.EqualTo(givenMessage.Name));
        }

        [Test]
        public void It_should_serialize_then_deserialize_UserRegistered_entirely()
        {
            // Given
            var givenMessage = new Fixture().Create<UserRegistered>();

            // When
            var json = JsonConvert.SerializeObject(givenMessage);
            var deserializedMessage = JsonConvert.DeserializeObject<UserRegistered>(json);

            // Then
            deserializedMessage.Should().BeEquivalentTo(givenMessage);
        }

        [TestCaseSource(nameof(AllMessageTypes))]
        public void It_should_serialize_then_deserialize(Type messageType)
        {
            // Given
            var givenMessage = new SpecimenContext(new Fixture()).Resolve(messageType); // https://github.com/AutoFixture/AutoFixture/issues/97#issuecomment-17064685

            // When
            var json = JsonConvert.SerializeObject(givenMessage);
            var deserializedMessage = JsonConvert.DeserializeObject(json, messageType);

            // Then
            deserializedMessage.Should().BeEquivalentTo(givenMessage);
        }

        private static IEnumerable<Type> AllMessageTypes =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && t.Namespace == "Gaev.Blog.Examples.Messages");
    }
}

namespace Gaev.Blog.Examples.Messages
{
    public class UserRegistered
    {
        public UserRegistered(Guid id, string email, string name)
        {
            Id = id;
            Email = email;
            Name = name;
        }

        public Guid Id { get; }
        public string Email { get; }
        public string Name { get; }
    }

    public class UserRegistered_V2
    {
        public UserRegistered_V2(Guid id, string email, string name)
        {
            Id = id;
            Login = email;
            Name = name;
        }

        public Guid Id { get; }
        public string Login { get; }
        public string Name { get; }
    }
}