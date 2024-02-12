using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WalkGuideFront.Controllers;
using WalkGuideFront.Models;
using WalkGuideFront.Models.Authentication.Interfaces;

namespace FrontTests
{
    [TestFixture]
    public class HomeControllerTests
    {
        [Test]
        public async Task CreateUser_EmailExists_ReturnsErrorMessage()
        {
            // Arrange
            var authenticationManagerMock = new Mock<IAuthenticationManager>();
            var homeController = new HomeController(authenticationManagerMock.Object);
            var userData = new AccountModel { Email = "test@test.com", Password = "Test123" };

            var exceptionMessage = "Exception occured while authenticating.\r\nUrl:" +
                "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}\r\n" +
                "Request Data: \"email\":\"er.ino@gl.c\",\"password\":\"---\",\"returnSecureToken\":true}\r\n" +
                "Response: {\r\n  " +
                "\"error\": {\r\n   " +
                " \"code\": 400,\r\n   " +
                " \"message\": \"EMAIL_EXISTS\",\r\n  " +
                "  \"errors\": [\r\n    " +
                "  {\r\n        \"message\": \"EMAIL_EXISTS\",\r\n" +
                "        \"domain\": \"global\",\r\n " +
                "       \"reason\": \"invalid\"\r\n " +
                "     }\r\n    ]\r\n  }\r\n}\r\n\r\n" +
                "Reason: EmailExists";

            authenticationManagerMock.Setup(x => x.CreateUserWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await homeController.CreateUser(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("This email is occupied", result.ViewData["ErrorMessage"]);
        }

        [Test]
        public async Task CreateUser_InvalidEmail_ReturnsErrorMessage()
        {
            // Arrange
            var authenticationManagerMock = new Mock<IAuthenticationManager>();
            var homeController = new HomeController(authenticationManagerMock.Object);
            var userData = new AccountModel { Email = "invalidemail", Password = "Test123" };

            var exceptionMessage = "Exception occured while authenticating.\r\nUrl:" +
                "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}\r\n" +
                "Request Data: \"email\":\"er.ino@gl.c\",\"password\":\"---\",\"returnSecureToken\":true}\r\n" +
                "Response: {\r\n  " +
                "\"error\": {\r\n   " +
                " \"code\": 400,\r\n   " +
                " \"message\": \"INVALID_EMAIL\",\r\n  " +
                "  \"errors\": [\r\n    " +
                "  {\r\n        \"message\": \"EMAIL_EXISTS\",\r\n" +
                "        \"domain\": \"global\",\r\n " +
                "       \"reason\": \"invalid\"\r\n " +
                "     }\r\n    ]\r\n  }\r\n}\r\n\r\n" +
                "Reason: EmailExists";

            authenticationManagerMock.Setup(x => x.CreateUserWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await homeController.CreateUser(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("This email is invalid", result.ViewData["ErrorMessage"]);
        }

        [Test]
        public async Task CreateUser_ValidEmail_RedirectsToSignIn()
        {
            // Arrange
            var authenticationManagerMock = new Mock<IAuthenticationManager>();
            var homeController = new HomeController(authenticationManagerMock.Object);
            var userData = new AccountModel { Email = "validemail@test.com", Password = "Test123" };

            FirebaseAuthProvider provider = new FirebaseAuthProvider(new FirebaseConfig("apiKey"));
            FirebaseAuthLink authLink = new FirebaseAuthLink(provider, new FirebaseAuth()) { FirebaseToken = "firebaseToken" };

            authenticationManagerMock.Setup(x => x.CreateUserWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ReturnsAsync(authLink);

            // Act
            var result = await homeController.CreateUser(userData) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("SignIn", result.ActionName);
        }
    }
}