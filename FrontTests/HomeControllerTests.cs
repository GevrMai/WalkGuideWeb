using Firebase.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WalkGuideFront.Controllers;
using WalkGuideFront.Models;
using WalkGuideFront.Models.Authentication.Interfaces;

namespace FrontTests
{
    [TestFixture]
    public partial class HomeControllerTests
    {
        private Mock<IAuthenticationManager>? authenticationManagerMock;
        private HomeController? homeController;
        private AccountModel? userData;
        private FirebaseAuthProvider? provider;
        private FirebaseAuthLink? authLink;

        private void SetUpWithFirebase(out Mock<IAuthenticationManager> authenticationManagerMock,
                        out HomeController homeController,
                        out AccountModel userData,
                        out FirebaseAuthProvider provider,
                        out FirebaseAuthLink authLink)
        {
            authenticationManagerMock = new Mock<IAuthenticationManager>();
            homeController = new HomeController(authenticationManagerMock.Object);
            userData = new AccountModel { Email = "somethingWrong", Password = "Test123" };

            provider = new FirebaseAuthProvider(new FirebaseConfig("apiKey"));
            authLink = new FirebaseAuthLink(provider, new FirebaseAuth())
            {
                FirebaseToken = "firebaseToken",
                User = new User() { Email = "validemail@test.com" }
            };
        }
        private void SetUp(out Mock<IAuthenticationManager> authenticationManagerMock,
                        out HomeController homeController,
                        out AccountModel userData)
        {
            authenticationManagerMock = new Mock<IAuthenticationManager>();
            homeController = new HomeController(authenticationManagerMock.Object);
            userData = new AccountModel { Email = "somethingWrong", Password = "Test123" };
        }

        private string GetExceptionMessage(string exceptionMessage)
        {
             return "Exception occured while authenticating.\r\nUrl:" +
                "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={0}\r\n" +
                "Request Data: \"email\":\"er.ino@gl.c\",\"password\":\"---\",\"returnSecureToken\":true}\r\n" +
                "Response: {\r\n  " +
                "\"error\": {\r\n   " +
                " \"code\": 400,\r\n   " +
                $" \"message\": \"{exceptionMessage}\",\r\n  " +
                "  \"errors\": [\r\n    " +
                "  {\r\n        \"message\": \"EMAIL_EXISTS\",\r\n" +
                "        \"domain\": \"global\",\r\n " +
                "       \"reason\": \"invalid\"\r\n " +
                "     }\r\n    ]\r\n  }\r\n}\r\n\r\n" +
                "Reason: EmailExists";
        }

        [Test]
        public async Task CreateUser_EmailExists_ReturnsErrorMessage()
        {
            // Arrange

            SetUp(out authenticationManagerMock, out homeController, out userData);

            var exceptionMessage = GetExceptionMessage("EMAIL_EXISTS");

            authenticationManagerMock.Setup(x => x.CreateUserWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await homeController.CreateUser(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewData["ErrorMessage"], Is.EqualTo("This email is occupied"));
        }

        [Test]
        public async Task CreateUser_InvalidEmail_ReturnsErrorMessage()
        {
            // Arrange

            SetUp(out authenticationManagerMock, out homeController, out userData);

            var exceptionMessage = GetExceptionMessage("INVALID_EMAIL");

            authenticationManagerMock.Setup(x => x.CreateUserWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await homeController.CreateUser(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewData["ErrorMessage"], Is.EqualTo("This email is invalid"));
        }

        [Test]
        public async Task CreateUser_ValidEmail_RedirectsToSignIn()
        {
            // Arrange

            SetUpWithFirebase(out authenticationManagerMock, out homeController, out userData, out provider, out authLink);

            authenticationManagerMock.Setup(x => x.CreateUserWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ReturnsAsync(authLink);

            // Act
            var result = await homeController.CreateUser(userData) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("SignIn"));
        }

        [Test]
        public async Task SignIn_ValidUserModel_RedirectsToIndex()
        {
            // Arrange

            SetUpWithFirebase(out authenticationManagerMock, out homeController, out userData, out provider, out authLink);

            authenticationManagerMock.Setup(x => x.SignInWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ReturnsAsync(authLink);

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockSession["Key"] = "value";
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            homeController.ControllerContext.HttpContext = mockHttpContext.Object;

            // Act
            var result = await homeController.SignIn(userData) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task SignIn_WrongEmailOrPassword_ReturnErrorMessage()
        {
            // Arrange

            SetUp(out authenticationManagerMock, out homeController, out userData);

            authenticationManagerMock.Setup(x => x.SignInWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ThrowsAsync(new FirebaseAuthException("reqUrl", "reqData", "ResData", new Exception()));

            // Act
            var result = await homeController.SignIn(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewData["ErrorMessage"], Is.EqualTo("Cannot sign in into account, check email and password"));
        }

        [Test]
        public async Task SignIn_UknownError_ReturnErrorMessage()
        {
            // Arrange

            SetUp(out authenticationManagerMock, out homeController, out userData);

            authenticationManagerMock.Setup(x => x.SignInWithEmailAndPasswordAsync(userData.Email, userData.Password))
                .ThrowsAsync(new Exception());

            // Act
            var result = await homeController.SignIn(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewData["ErrorMessage"], Is.EqualTo("Unknown error"));
        }

        [Test]
        public async Task AccountPageResetPasswordWithEmail_ValidEmail_ReturnSuccessMessage()
        {
            // Arrange

            SetUpWithFirebase(out authenticationManagerMock, out homeController, out userData, out provider, out authLink);

            authenticationManagerMock.Setup(x => x.SendPasswordResetEmailAsync(userData.Email));

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockSession["_UserEmail"] = "value";
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            homeController.ControllerContext.HttpContext = mockHttpContext.Object;

            // Act
            var result = await homeController.AccountPage(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewData["EmailSent"], Is.EqualTo("An email with instructions to change your password has been sent to your email address"));
        }

        [Test]
        public async Task AccountPageResetPasswordWithEmail_SessionDoesNotHaveEmail_ReturnErrorStatusMessage()
        {
            // Arrange

            SetUpWithFirebase(out authenticationManagerMock, out homeController, out userData, out provider, out authLink);

            authenticationManagerMock.Setup(x => x.SendPasswordResetEmailAsync(userData.Email));

            Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
            MockHttpSession mockSession = new MockHttpSession();
            mockSession["_NoSuchKey"] = "value";
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            homeController.ControllerContext.HttpContext = mockHttpContext.Object;

            // Act
            var result = await homeController.AccountPage(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewData["EmailSent"], Is.EqualTo("Problem with your user session"));
        }

        [Test]
        public async Task AccountPageResetPasswordWithEmail_ThrowsException_ReturnErrorStatusMessage()
        {
            // Arrange

            SetUpWithFirebase(out authenticationManagerMock, out homeController, out userData, out provider, out authLink);

            authenticationManagerMock.Setup(x => x.SendPasswordResetEmailAsync(userData.Email))
                .ThrowsAsync(new Exception("Error"));

            // Act
            var result = await homeController.AccountPage(userData) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.ViewData["EmailSent"], Is.EqualTo("Uknown error"));
        }
    }
}