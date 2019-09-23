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
        private IProfile _profileDao;
        private INotification _notification;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private IOtpService _otpService;

        [SetUp]
        public void SetUp()
        {
            _profileDao = Substitute.For<IProfile>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _hash = Substitute.For<IHash>();
            _logger = Substitute.For<ILogger>();
            _otpService = Substitute.For<IOtpService>();
        }

        [Test]
        public void is_valid()
        {
            _profileDao.GetHashedPasswordFromDb(DefaultAccountId).Returns(DefaultHashedPassword);
            _hash.Hash(DefaultPassword).Returns(DefaultHashedPassword);
            _otpService.GetCurrentOtp(DefaultAccountId).Returns(DefaultOtp);

            var isValid = WhenVerify();
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify()
        {
            var authenticationService =
                new AuthenticationService(_notification, _failedCounter, _logger, _profileDao, _hash, _otpService);
            var isValid = authenticationService.Verify(DefaultAccountId, DefaultPassword, DefaultOtp);
            return isValid;
        }
    }
}