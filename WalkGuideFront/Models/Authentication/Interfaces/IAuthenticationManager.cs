using Firebase.Auth;

namespace WalkGuideFront.Models.Authentication.Interfaces
{
    public interface IAuthenticationManager
    {
        public Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password);
        public Task<FirebaseAuthLink> CreateUserWithEmailAndPasswordAsync(string email, string password);
        public Task SendPasswordResetEmailAsync(string email);
    }
}
