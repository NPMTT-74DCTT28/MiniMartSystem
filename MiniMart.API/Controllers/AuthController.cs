using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.API.Security;
using MiniMart.Shared.DTOs.Auth;
using MiniMart.Shared.DTOs.Users;

namespace MiniMart.API.Controllers
{
    /// <summary>Xử lý đăng nhập của nhân viên và phát hành JWT cho Client.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(AppDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Đăng nhập bằng tên đăng nhập và mật khẩu.
        /// Trả về 200 kèm access token khi thành công, 401 khi sai thông tin, 403 khi tài khoản bị vô hiệu hóa.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Username == loginRequestDto.Username);

            // Không phân biệt "sai tên đăng nhập" và "sai mật khẩu" để tránh dò tên tài khoản.
            if (user is null || !PasswordHelper.Verify(loginRequestDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            if (!user.IsActive)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản đã bị vô hiệu hóa." });
            }

            var (accessToken, expiresAtUtc) = _jwtTokenService.CreateAccessToken(user);

            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                ExpiresAtUtc = expiresAtUtc,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    IsActive = user.IsActive
                }
            };

            return Ok(response);
        }
    }
}
