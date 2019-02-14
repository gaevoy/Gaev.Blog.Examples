using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using PlantUml.Net;
using Serilog;
using Serilog.Events;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Gaev.Blog.Examples
{
    public class UserStateTests
    {
        private ILogger _logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}")
            .MinimumLevel.Debug()
            .CreateLogger();

        [Test]
        public void It_should_login()
        {
            // Given
            var user = new TestUser(_logger);

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
            var user = new TestUser(_logger) {NumberOfAttempts = 2}.HavingState<UserAttemptsToLogin>();

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
            var user = new TestUser(_logger) {Captcha = captcha}.HavingState<UserInputsCaptcha>();

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
            var user = new TestUser(_logger).HavingState<UserInputsCaptcha>();

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
            var user = new TestUser(_logger).HavingState<UserIsAuthorized>();

            // When
            user.State.Logout();

            // Then
            Assert.That(user.State.HasAccess, Is.False);
            Assert.That(user.State, Is.TypeOf<UserAttemptsToLogin>());
        }

        [Test]
        public void PlantUml_should_build_state_diagram()
        {
            // Given
            var planUmlCode = new List<string>();
            _logger = Substitute.For<ILogger>();
            _logger.IsEnabled(LogEventLevel.Debug).Returns(true);
            _logger.When(e => e.Debug(Arg.Any<string>())).Do(e => planUmlCode.Add(e.Arg<string>()));

            // When
            It_should_login();
            It_should_show_captcha();
            It_should_validate_captcha();
            It_should_be_blocked();
            It_should_logout();

            // Then
            var veryFirstState = planUmlCode[0].Substring(0, planUmlCode[0].IndexOf(" --> "));
            planUmlCode.Add($"[*] --> {veryFirstState}");
            var code = string.Join("\n", planUmlCode.Distinct());
            var diagramUrl = new RendererFactory()
                .CreateRenderer()
                .RenderAsUri(code, OutputFormat.Png);
            Console.WriteLine(diagramUrl);
        }

        public class TestUser : User
        {
            private readonly ILogger _logger;

            public TestUser(ILogger logger)
            {
                _logger = logger;
            }

            public override void OnStateChanged(UserState prev, UserState next)
            {
                if (_logger.IsEnabled(LogEventLevel.Debug))
                {
                    var callingMethod = new StackTrace().GetFrame(2).GetMethod();
                    _logger.Debug($"{prev.GetType().Name} --> {next.GetType().Name} : {callingMethod.Name}");
                }
            }
        }
    }
}