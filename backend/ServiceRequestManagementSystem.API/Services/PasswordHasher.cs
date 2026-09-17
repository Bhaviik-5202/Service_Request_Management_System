using System.Security.Cryptography;

namespace ServiceRequestManagementSystem.API.Services
{
    public interface IPasswordHasher
    {
        (string Hash, string Salt) HashPassword(string password);
        bool VerifyPassword(string password, string storedHash, string storedSalt);
    }

    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 100000;

        public (string Hash, string Salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            var salt = Convert.ToBase64String(saltBytes);

            var key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            var hash = Convert.ToBase64String(key);

            return (hash, salt);
        }

        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(storedHash) ||
                string.IsNullOrWhiteSpace(storedSalt))
            {
                return false;
            }

            try
            {
                var saltBytes = Convert.FromBase64String(storedSalt);
                var key = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    saltBytes,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    KeySize);

                var computedHash = Convert.ToBase64String(key);

                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(computedHash),
                    Convert.FromBase64String(storedHash));
            }
            catch
            {
                return false;
            }
        }
    }
}
