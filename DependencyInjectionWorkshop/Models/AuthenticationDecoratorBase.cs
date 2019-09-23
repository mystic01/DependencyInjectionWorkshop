namespace DependencyInjectionWorkshop.Models
{
    public abstract class AuthenticationDecoratorBase : IAuthentication
    {
        protected IAuthentication _authentication;

        protected AuthenticationDecoratorBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public abstract bool Verify(string accountId, string password, string otp);
    }
}