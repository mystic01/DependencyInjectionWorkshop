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

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            _failedCounter.Received(1).ResetFailedCount(DefaultAccountId);
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