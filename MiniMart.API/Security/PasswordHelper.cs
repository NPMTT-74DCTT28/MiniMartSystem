using System.Security.Cryptography;

namespace MiniMart.API.Security
{
    /// <summary>
    /// Băm và kiểm tra mật khẩu nhân viên bằng PBKDF2 (HMAC-SHA256).
    /// Không bao giờ lưu mật khẩu dạng thô vào cột <c>User.PasswordHash</c>.
    /// </summary>
    /// <remarks>
    /// Chuỗi băm có dạng <c>PBKDF2$&lt;số vòng lặp&gt;$&lt;salt base64&gt;$&lt;hash base64&gt;</c>,
    /// độ dài khoảng 83 ký tự nên nằm gọn trong cột <c>nvarchar(255)</c>.
    /// Salt được sinh ngẫu nhiên cho từng tài khoản nên hai mật khẩu giống nhau cho ra chuỗi băm khác nhau.
    /// </remarks>
    public static class PasswordHelper
    {
        private const string AlgorithmPrefix = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        /// <summary>Băm mật khẩu dạng thô thành chuỗi an toàn để lưu vào database.</summary>
        public static string Hash(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

            return string.Join('$',
                AlgorithmPrefix,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key));
        }

        /// <summary>
        /// Kiểm tra mật khẩu dạng thô có khớp với chuỗi băm đã lưu hay không.
        /// Trả về <c>false</c> (thay vì ném lỗi) khi chuỗi băm sai định dạng.
        /// </summary>
        public static bool Verify(string? password, string? passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            var parts = passwordHash.Split('$');

            if (parts.Length != 4 || !string.Equals(parts[0], AlgorithmPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedKey;

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedKey = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length == 0 || expectedKey.Length == 0)
            {
                return false;
            }

            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedKey.Length);

            // So sánh theo thời gian hằng định để hạn chế tấn công timing.
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
    }
}
