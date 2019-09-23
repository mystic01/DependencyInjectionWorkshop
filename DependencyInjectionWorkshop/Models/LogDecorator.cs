namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogDecorator(IAuthentication authentication, ILogger logger, IFailedCounter failedCounter) : base(authentication)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        private void Info(string accountId)
        {
            var message = $"accountId:{accountId} failed times:{_failedCounter.GetFailedCount(accountId)}";
            _logger.Info(message);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid)
            {
                Info(accountId);
            }

            return isValid;
        }
    }
}