using System;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        private void Send(string accountId)
        {
            _notification.Send($"{accountId} try to login failed.");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            Send(accountId);
            return isValid;
        }
    }

    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly INotification _slackAdapter;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _nLogAdapter;
        private readonly IProfile _profileDao;
        private readonly IHash _sha256Adapter;
        private readonly IOtpService _otpService;

        public AuthenticationService(INotification slackAdapter, IFailedCounter failedCounter, ILogger nLogAdapter, IProfile profileDao, IHash sha256Adapter, IOtpService otpService)
        {
            _slackAdapter = slackAdapter;
            _failedCounter = failedCounter;
            _nLogAdapter = nLogAdapter;
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
        }

        public AuthenticationService()
        {
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
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
                _failedCounter.AddFailedCount(accountId);

                var message = $"accountId:{accountId} failed times:{_failedCounter.GetFailedCount(accountId)}";
                _nLogAdapter.Info(message);

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}