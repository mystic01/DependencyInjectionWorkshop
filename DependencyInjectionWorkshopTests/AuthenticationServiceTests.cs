using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "mystic";
        private const string DefaultHashedPassword = "hashed_password";
        private const string DefaultPassword = "password";
        private const string DefaultOtp = "123456";
        private const int DefaultFailedCount = 87;
        private IProfile _profileDao;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private IOtpService _otpService;
        private IAuthentication _authenticationService;

        [SetUp]
        public void SetUp()
        {
            _profileDao = Substitute.For<IProfile>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _hash = Substitute.For<IHash>();
            _logger = Substitute.For<ILogger>();
            _otpService = Substitute.For<IOtpService>();
            _authenticationService = new AuthenticationService(_failedCounter, _profileDao, _hash, _otpService);
            _authenticationService = new NotificationDecorator(_authenticationService, _notification);
            _authenticationService = new LogDecorator(_authenticationService, _logger, _failedCounter);
        }

        [Test]
        public void is_valid()
        {
            _profileDao.GetHashedPasswordFromDb(DefaultAccountId).Returns(DefaultHashedPassword);
            _hash.Hash(DefaultPassword).Returns(DefaultHashedPassword);
            _otpService.GetCurrentOtp(DefaultAccountId).Returns(DefaultOtp);

            var authenticationService =
                new AuthenticationService(_failedCounter, _profileDao, _hash, _otpService);
            var isValid = authenticationService.Verify(DefaultAccountId, DefaultPassword, DefaultOtp);
            Assert.IsTrue(isValid);
        }

        private bool WhenValid()
        {
            _profileDao.GetHashedPasswordFromDb(DefaultAccountId).Returns(DefaultHashedPassword);
            _hash.Hash(DefaultPassword).Returns(DefaultHashedPassword);
            _otpService.GetCurrentOtp(DefaultAccountId).Returns(DefaultOtp);

            var isValid = _authenticationService.Verify(DefaultAccountId, DefaultPassword, DefaultOtp);
            return isValid;
        }

        [Test]
        public void is_invalid()
        {
            _profileDao.GetHashedPasswordFromDb(DefaultAccountId).Returns(DefaultHashedPassword);
            _hash.Hash(DefaultPassword).Returns(DefaultHashedPassword);
            _otpService.GetCurrentOtp(DefaultAccountId).Returns(DefaultOtp);

            var authenticationService =
                new AuthenticationService(_failedCounter, _profileDao, _hash, _otpService);
            var isValid = authenticationService.Verify(DefaultAccountId, "wrong password", DefaultOtp);
            Assert.IsFalse(isValid);
        }

        private bool WhenInvalid()
        {
            _profileDao.GetHashedPasswordFromDb(DefaultAccountId).Returns(DefaultHashedPassword);
            _hash.Hash(DefaultPassword).Returns(DefaultHashedPassword);
            _otpService.GetCurrentOtp(DefaultAccountId).Returns(DefaultOtp);

            var isValid = _authenticationService.Verify(DefaultAccountId, "wrong password", DefaultOtp);
            return isValid;
        }

        [Test]
        public void reset_failed_counter_when_valid()
        {
            WhenValid();
            _failedCounter.Received(1).ResetFailedCount(DefaultAccountId);
        }

        [Test]
        public void add_failed_counter_when_invalid()
        {
            WhenInvalid();
            _failedCounter.Received(1).AddFailedCount(DefaultAccountId);
        }

        [Test]
        public void log_failed_counter_when_invalid()
        {
            _failedCounter.GetFailedCount(DefaultAccountId).Returns(DefaultFailedCount);
            WhenInvalid();
            _logger.Received(1).Info(Arg.Is<string>(m =>
                m.Contains(DefaultAccountId) && m.Contains(DefaultFailedCount.ToString())));
        }

        [Test]
        public void send_message_when_invalid()
        {
            WhenInvalid();
            _notification.Received(1).Send(Arg.Is<string>(m => m.Contains(DefaultAccountId)));
        }

        [Test]
        public void throw_exception_when_account_is_locked()
        {
            _failedCounter.IsAccountLocked(DefaultAccountId).Returns(true);
            TestDelegate action = () => WhenValid();
            Assert.Throws<FailedTooManyTimesException>(action);
        }

        [Test]
        public void add_and_then_log_failed_count()
        {
            WhenInvalid();
            Received.InOrder(() =>
            {
                _failedCounter.AddFailedCount(DefaultAccountId);
                _logger.Info(Arg.Any<string>());
            });
        }
    }
}