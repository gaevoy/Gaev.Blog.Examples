using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gaev.Blog.Examples
{
    public class User
    {
        private UserState _userState;

        [NotMapped]
        public UserState State
        {
            get => _userState ?? (_userState = UserState.New(StateType, this));
            set => _userState = value;
        }

        public int Id { get; set; }
        public string StateType { get; set; }
        public int NumberOfAttempts { get; set; }
        public string Captcha { get; set; }
        public DateTimeOffset? BlockedUntil { get; set; }
    }

    public class UserAttemptsToLogin : UserState
    {
        protected override void OnStart()
        {
            User.NumberOfAttempts = 0;
            User.Captcha = null;
        }

        public override void Login(string password)
        {
            if (password == "test")
                Become(new UserIsAuthorized());
            else
            {
                User.NumberOfAttempts++;
                if (User.NumberOfAttempts > 2)
                    Become(new UserInputsCaptcha());
            }
        }
    }

    public class UserIsAuthorized : UserState
    {
        public override bool HasAccess => true;

        public override void Logout()
        {
            Become(new UserAttemptsToLogin());
        }
    }

    public class UserInputsCaptcha : UserState
    {
        protected override void OnStart()
        {
            User.Captcha = Guid.NewGuid().ToString();
        }

        public override void InputCaptcha(string captcha)
        {
            if (captcha == User.Captcha)
                Become(new UserAttemptsToLogin());
            else
                Become(new UserIsBlocked());
        }
    }

    public class UserIsBlocked : UserState
    {
        protected override void OnStart()
        {
            User.BlockedUntil = DateTimeOffset.UtcNow.AddHours(1);
        }
    }

    public abstract class UserState
    {
        protected User User { get; private set; }
        public virtual void Login(string password) => throw new InvalidOperationException();
        public virtual void InputCaptcha(string captcha) => throw new InvalidOperationException();
        public virtual void Logout() => throw new InvalidOperationException();
        public virtual bool HasAccess => false;

        protected virtual void OnStart()
        {
        }

        protected void Become(UserState next)
        {
            next.User = User;
            next.OnStart();
            User.StateType = next.GetType().Name;
            User.State = next;
        }

        public static UserState New(string type, User user)
        {
            switch (type)
            {
                case nameof(UserIsAuthorized): return new UserIsAuthorized {User = user};
                case nameof(UserInputsCaptcha): return new UserInputsCaptcha {User = user};
                case nameof(UserIsBlocked): return new UserIsBlocked {User = user};
                default: return new UserAttemptsToLogin {User = user};
            }
        }
    }
}