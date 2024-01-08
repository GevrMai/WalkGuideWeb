using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using WalkGuideFront.Models;
using WalkGuideFront.Models.Attributes;

namespace WalkGuideFront.Controllers
{
    public class HomeController : Controller
    {
        private const string ApiKey = "";
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(AccountModel userData)
        {
            try
            {
                FirebaseAuthProvider firebaseAuthProvider = new(new FirebaseConfig(ApiKey));

                var authLink = await firebaseAuthProvider.SignInWithEmailAndPasswordAsync(userData.Email, userData.Password);

                if (authLink.FirebaseToken != null)
                {
                    HttpContext.Session.SetString("_UserToken", authLink.FirebaseToken);
                    HttpContext.Session.SetString("_UserEmail", authLink.User.Email); // Saving an Email to use it in View

                    return RedirectToAction("Index");
                }
            }
            catch (FirebaseAuthException ex)
            {
                ViewBag.ErrorMessage = "Cannot sign in into account, check email and password";
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine(ex);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine(ex);
            }

            return View(userData);
        }

        public async Task<IActionResult> LogOut()
        {
            HttpContext.Session.Remove("_UserToken");
            HttpContext.Session.Remove("_UserEmail");
            return RedirectToAction("Index");
        }

        [CustomAuthorization]
        public IActionResult PostData()
        {
            return View();
        }

        [CustomAuthorization]
        public async Task<IActionResult> PostDataSend(PostDataToApi data)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:7158/");

                    var authToken = HttpContext.Session.GetString("_UserToken");

                    if (!string.IsNullOrEmpty(authToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                        HttpResponseMessage response = await client.GetAsync("TestAuth");

                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(responseString);
                        }
                        else
                        {
                            Console.WriteLine("Ошибка: " + response.StatusCode);
                        }
                    }   

                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine(ex);
            }

            return RedirectToAction("PostData");
        }

        [CustomAuthorization]
        public IActionResult AccountPage()
        {
            return View();
        }

        [HttpPost]
        [CustomAuthorization]
        public async Task<IActionResult> AccountPage(AccountModel userData)
        {
            try
            {
                FirebaseAuthProvider firebaseAuthProvider = new(new FirebaseConfig(ApiKey));
                await firebaseAuthProvider.SendPasswordResetEmailAsync(HttpContext.Session.GetString("_UserEmail"));
                ViewBag.EmailSent = "An email with instructions to change your password has been sent to your email address";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return View();
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(AccountModel userData)
        {
            try
            {
                FirebaseAuthProvider firebaseAuthProvider = new(new FirebaseConfig(ApiKey));

                var authLink = await firebaseAuthProvider.
                    CreateUserWithEmailAndPasswordAsync(userData.Email, userData.Password);

                if (authLink.FirebaseToken != null)
                    return RedirectToAction("SignIn");
            }
            catch (Exception ex)
            {
                string pattern = @"""message""\s*:\s*""(.*?)""";
                Match match = Regex.Match(ex.Message, pattern);

                if (match.Success)
                {
                    if (match.Groups[1].Value == "EMAIL_EXISTS")
                        ViewBag.ErrorMessage = "This email is occupied";
                    else if(match.Groups[1].Value == "INVALID_EMAIL")
                        ViewBag.ErrorMessage = "This email is invalid";
                }

                Console.WriteLine($"{ex.Message}");
                Console.WriteLine(ex);
            }

            return View(userData);
            
        }

        //default error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}