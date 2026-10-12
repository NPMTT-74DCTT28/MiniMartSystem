using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniMart.API.Options;
using MiniMart.Shared.Entities;

namespace MiniMart.API.Security
{
    /// <summary>Phát hành access token cho tài khoản nhân viên sau khi đăng nhập thành công.</summary>
    public interface IJwtTokenService
    {
        /// <summary>Tạo access token cho <paramref name="user"/> và trả về cả thời điểm hết hạn (UTC).</summary>
        (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);
    }

    /// <inheritdoc cref="IJwtTokenService" />
    public class JwtTokenService : IJwtTokenService
    {
        /// <summary>Tên claim vai trò. JwtBearer tự ánh xạ sang <see cref="ClaimTypes.Role"/> khi xác thực.</summary>
        private const string RoleClaimType = "role";

        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value.Validate();
        }

        public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var now = DateTime.UtcNow;
            var expiresAtUtc = now.AddMinutes(_jwtSettings.ExpiryMinutes);

            // Claims dùng cho phân quyền ở API và truy vết tài khoản đang đăng nhập.
            // Dùng tên claim ngắn ("sub", "role", ...) để payload gọn và dễ đọc.
            // Phía API, JwtBearer tự ánh xạ "role" -> ClaimTypes.Role nên [Authorize(Roles = "Admin")] vẫn hoạt động.
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(RoleClaimType, user.Role.ToString())
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAtUtc,
                signingCredentials: signingCredentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }
    }
}
