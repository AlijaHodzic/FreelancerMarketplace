using System.Security.Cryptography;

namespace Freelance.Infrastructure.Services
{
    public static class RefreshTokenGenerator
    {
        public static string GenerateToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
