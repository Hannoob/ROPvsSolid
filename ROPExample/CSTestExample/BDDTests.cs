using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SOLIDExample.BuisinessLogic;
using SOLIDExample.DomainObjects;
using SOLIDExample.Interfaces;
using FluentAssertions;
namespace CSTestExample
{
    [TestFixture]
    public class Given_a_valid_username_and_password_When_a_user_authenticates_and_the_db_is_available
    {
        private User response;

        [OneTimeSetUp]
        public void setup()
        {
            var user = new User()
            {
                UserId = "1",
                Username = "username",
                Password = "password",
                Email = "test@test.com"
            };

            var logger = new Mock<ILogger<UserHandler>>();

            var userRepo = new Mock<IUserRepo>();
            userRepo
                .Setup(repo => repo.GetUserDetails(user.Username))
                .Returns(user);

            var passwordUtils = new Mock<IPasswordUtils>();
            passwordUtils
                .Setup(util => util.ComparePasswords("password", user.Password))
                .Returns(true);

            var emailService = new Mock<IEmailService>();
            emailService
                .Setup(service => service.EmailClient(user.Email, It.IsAny<string>()));

            var handler = new UserHandler(logger.Object, userRepo.Object, passwordUtils.Object, emailService.Object);

            response = handler.AuthenticateUser("username", "password");
        }

        [Test]
        public void Then_The_Response_Object_Should_Have_The_Correct_Username()
        {
            response.Username.Should().Be("username");
        }

        [Test]
        public void Then_The_Response_Object_Should_Have_The_Correct_Email()
        {
            response.Email.Should().Be("test@test.com");
        }

        [Test]
        public void Then_The_Response_Object_Should_Have_The_Correct_UserId()
        {
            response.UserId.Should().Be("1");
        }
    }
}
