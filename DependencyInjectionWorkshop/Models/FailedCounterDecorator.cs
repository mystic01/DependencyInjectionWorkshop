namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        private void AddFailedCounter(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (isValid)
            {
                ResetFailedCounter(accountId);
            }
            else
            {
                AddFailedCounter(accountId);
            }

            return isValid;
        }

        public void ResetFailedCounter(string accountId)
        {
            _failedCounter.ResetFailedCount(accountId);
        }
    }
}