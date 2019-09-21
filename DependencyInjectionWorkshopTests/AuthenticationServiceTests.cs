using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "Mystic";
        private const string DefaultHashedPassword = "hashed_password";
        private const string DefaultInputPassword = "password";
        private const string DefaultOtp = "123456";
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private ILogger _logger;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void Setup()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _logger = Substitute.For<ILogger>();
            _authenticationService = new AuthenticationService(_profile, _hash, _otpService, _notification, _failedCounter, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");

            ShouldBeInvalid(isValid);
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            return WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            _failedCounter.Received(1).ResetFailedCount(DefaultAccountId);
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            _failedCounter.Received(1).AddFailedCount(DefaultAccountId);
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            _failedCounter.GetFailedCount(DefaultAccountId).Returns(88);
            WhenInvalid();
            _logger.Received(1).Info(Arg.Is<string>(m => m.Contains(DefaultAccountId) && m.Contains("88")));
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            _notification.Received(1).Send(Arg.Is<string>(m => m.Contains(DefaultAccountId)));
        }

        [Test]
        public void account_is_locked()
        {
            _failedCounter.GetIsLocked(DefaultAccountId).Returns(true);
            TestDelegate action = () => WhenValid();
            Assert.Throws<FailedTooManyTimesException>(action);
        }

        private void WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string accountId, string inputPassword, string otp)
        {
            return _authenticationService.Verify(accountId, inputPassword, otp);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenHash(string input, string hashedInput)
        {
            _hash.Compute(input).Returns(hashedInput);
        }

        private void GivenPassword(string accountId, string input)
        {
            _profile.GetPassword(accountId).Returns(input);
        }
    }
}