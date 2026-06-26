namespace VolunTrack.Services
{
    using BCrypt.Net;

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.EnhancedHashPassword(
                password,
                HashType.SHA512,
                workFactor: 12
            );
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            return BCrypt.EnhancedVerify(
                providedPassword,
                hashedPassword,
                HashType.SHA512
            );
        }
    }
}
