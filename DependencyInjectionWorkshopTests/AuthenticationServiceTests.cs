using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repositories;
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
            var hash = Substitute.For<IHash>();
            var otpService = Substitute.For<IOtpService>();
            var notification = Substitute.For<INotification>();
            var failedCounter = Substitute.For<IFailedCounter>();
            var logger = Substitute.For<ILogger>();

            profile.GetPassword("Mystic").Returns("hashed_password");
            hash.Compute("password").Returns("hashed_password");
            otpService.GetCurrentOtp("Mystic").Returns("123456");

            var authenticationService = new AuthenticationService(profile, hash, otpService, notification, failedCounter, logger);

            var isValid = authenticationService.Verify("Mystic", "password", "123456");

            Assert.IsTrue(isValid);
        }
    }
}