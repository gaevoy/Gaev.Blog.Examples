using System;
using System.Data.Entity;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace Gaev.Blog.Examples
{
    public class UserTests
    {
        private const string ConnectionString = "server=localhost;database=tempdb;UID=sa;PWD=sa123";
        private readonly Fixture _fixture = new Fixture();

        [Test]
        public void It_should_login()
        {
            // Given
            var user = new User();

            // When
            user.State.Login("test");

            // Then
            Assert.That(user.State.HasAccess, Is.True);
            Assert.That(user.State, Is.TypeOf<UserIsAuthorized>());
        }

        [Test]
        public void It_should_show_captcha()
        {
            // Given
            var user = new User {NumberOfAttempts = 2}.HavingState<UserAttemptsToLogin>();

            // When
            user.State.Login("fail");

            // Then
            Assert.That(user.State.HasAccess, Is.False);
            Assert.That(user.Captcha, Is.Not.Null);
            Assert.That(user.State, Is.TypeOf<UserInputsCaptcha>());
        }

        [Test]
        public void It_should_validate_captcha()
        {
            // Given
            var captcha = Guid.NewGuid().ToString();
            var user = new User {Captcha = captcha}.HavingState<UserInputsCaptcha>();

            // When
            user.State.InputCaptcha(captcha);

            // Then
            Assert.That(user.Captcha, Is.Null);
            Assert.That(user.State, Is.TypeOf<UserAttemptsToLogin>());
        }

        [Test]
        public void It_should_be_blocked()
        {
            // Given
            var user = new User().HavingState<UserInputsCaptcha>();

            // When
            user.State.InputCaptcha("wrong");

            // Then
            var now = DateTimeOffset.UtcNow;
            Assert.That(user.BlockedUntil, Is.EqualTo(now.AddHours(1)).Within(100).Milliseconds);
            Assert.That(user.State, Is.TypeOf<UserIsBlocked>());
        }

        [Test]
        public void It_should_logout()
        {
            // Given
            var user = new User().HavingState<UserIsAuthorized>();

            // When
            user.State.Logout();

            // Then
            Assert.That(user.State.HasAccess, Is.False);
            Assert.That(user.State, Is.TypeOf<UserAttemptsToLogin>());
        }

        [Test]
        public void It_should_throw_error_in_UserAttemptsToLogin_state()
        {
            // Given
            var user = new User().HavingState<UserAttemptsToLogin>();

            // When
            void InputCaptcha() => user.State.InputCaptcha("");
            void Logout() => user.State.Logout();

            // Then
            Assert.That(InputCaptcha, Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.That(Logout, Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void It_should_throw_error_in_UserIsAuthorized_state()
        {
            // Given
            var user = new User().HavingState<UserIsAuthorized>();

            // When
            void Login() => user.State.Login("");
            void InputCaptcha() => user.State.InputCaptcha("");

            // Then
            Assert.That(Login, Throws.Exception.TypeOf<InvalidOperationException>());
            Assert.That(InputCaptcha, Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task It_should_insert_state()
        {
            // Given
            var user = _fixture.Build<User>()
                .Without(e => e.Id)
                .Without(e => e.State)
                .Create();

            // When
            using (var db = new Db(ConnectionString))
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            // Then
            using (var db = new Db(ConnectionString))
            {
                var actual = await db.Users.FindAsync(user.Id);
                actual.Should().BeEquivalentTo(user);
            }
        }

        [Test]
        public async Task It_should_update_state()
        {
            // Given
            User user;
            using (var db = new Db(ConnectionString))
            {
                user = new User();
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            // When
            using (var db = new Db(ConnectionString))
            {
                user = _fixture.Build<User>()
                    .With(e => e.Id, user.Id)
                    .Without(e => e.State)
                    .Create();
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            // Then
            using (var db = new Db(ConnectionString))
            {
                var actual = await db.Users.FindAsync(user.Id);
                actual.Should().BeEquivalentTo(user);
            }
        }
    }

    public class Db : DbContext
    {
        public DbSet<User> Users { get; set; }

        public Db(string connectionString) : base(connectionString)
        {
        }
    }

    public static class UserExt
    {
        public static User HavingState<T>(this User user)
        {
            user.State = UserState.New(typeof(T).Name, user);
            return user;
        }
    }
}