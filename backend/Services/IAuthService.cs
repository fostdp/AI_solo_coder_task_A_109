using SculptureMonitor.Models;

namespace SculptureMonitor.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<User> RegisterAsync(User user, string password);
        Task<bool> ValidateUserAsync(string username, string password);
        string GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
