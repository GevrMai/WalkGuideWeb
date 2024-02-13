using Firebase.Auth;
using WalkGuideFront.Models.Authentication.Interfaces;

namespace WalkGuideFront.Models.Authentication
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private const string ApiKey = "*api key*";

        public async Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            FirebaseAuthProvider firebaseAuthProvider = new(new FirebaseConfig(ApiKey));

            return await firebaseAuthProvider.SignInWithEmailAndPasswordAsync(email, password);
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            FirebaseAuthProvider firebaseAuthProvider = new (new FirebaseConfig(ApiKey));

            await firebaseAuthProvider.SendPasswordResetEmailAsync(email);
        }

        public async Task<FirebaseAuthLink> CreateUserWithEmailAndPasswordAsync(string email, string password)
        {
            FirebaseAuthProvider firebaseAuthProvider = new(new FirebaseConfig(ApiKey));

            return await firebaseAuthProvider.CreateUserWithEmailAndPasswordAsync(email, password);
        }
    }
}
