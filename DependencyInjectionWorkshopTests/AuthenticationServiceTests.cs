using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var notification = Substitute.For<INotification>();
            var failedCounter = Substitute.For<IFailedCounter>();
            var hash = Substitute.For<IHash>();
            var logger = Substitute.For<ILogger>();
            var otpService = Substitute.For<IOtpService>();

            profile.GetHashedPasswordFromDb("mystic").Returns("hashed_password");
            hash.Hash("password").Returns("hashed_password");
            otpService.GetCurrentOtp("mystic").Returns("123456");

            var authenticationService = new AuthenticationService(notification, failedCounter, logger, profile, hash, otpService);
            var isValid = authenticationService.Verify("mystic", "password", "123456");
            Assert.IsTrue(isValid);
        }
    }
}