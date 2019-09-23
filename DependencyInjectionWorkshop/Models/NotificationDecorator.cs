namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationDecorator : AuthenticationDecoratorBase
    {
        private readonly INotification _notification;

        public AuthenticationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _authentication = authentication;
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send($"{accountId} try to login failed.");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            Send(accountId);
            return isValid;
        }
    }
}