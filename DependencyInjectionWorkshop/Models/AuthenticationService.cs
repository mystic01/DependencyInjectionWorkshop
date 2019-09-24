using System;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        private void AddFailedCounter(string accountId)
        {
            _failedCounter.AddFailedCount(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid)
            {
                AddFailedCounter(accountId);
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profileDao;
        private readonly IHash _sha256Adapter;
        private readonly IOtpService _otpService;
        private readonly FailedCounterDecorator _failedCounterDecorator;

        public AuthenticationService(IFailedCounter failedCounter, IProfile profileDao, IHash sha256Adapter, IOtpService otpService)
        {
            _failedCounter = failedCounter;
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
        }

        public AuthenticationService()
        {
            _failedCounter = new FailedCounter();
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            if (_failedCounter.IsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var passwordFromDb = _profileDao.GetHashedPasswordFromDb(accountId);

            var hashedPassword = _sha256Adapter.Hash(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (hashedPassword == passwordFromDb && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(accountId);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}