using System;
using static Gaev.Blog.Examples.LegacyUser.UserState;

namespace Gaev.Blog.Examples
{
    public class LegacyUser
    {
        public int NumberOfAttempts { get; set; }
        public string Captcha { get; set; }
        public DateTimeOffset? BlockedUntil { get; set; }
        public UserState State { get; set; }

        public void Login(string password)
        {
            if (State != AttemptsToLogin) throw new InvalidOperationException();
            if (password == "test")
                State = IsAuthorized;
            else
            {
                NumberOfAttempts++;
                if (NumberOfAttempts > 2)
                {
                    Captcha = Guid.NewGuid().ToString();
                    State = InputsCaptcha;
                }
            }
        }

        public void InputCaptcha(string captcha)
        {
            if (State != InputsCaptcha) throw new InvalidOperationException();
            if (captcha == Captcha)
            {
                NumberOfAttempts = 0;
                State = AttemptsToLogin;
            }
            else
            {
                BlockedUntil = DateTimeOffset.UtcNow.AddHours(1);
                State = IsBlocked;
            }
        }

        public void Logout()
        {
            if (State != IsAuthorized) throw new InvalidOperationException();
            NumberOfAttempts = 0;
            State = AttemptsToLogin;
        }

        public enum UserState
        {
            AttemptsToLogin,
            IsAuthorized,
            InputsCaptcha,
            IsBlocked
        }
    }
}