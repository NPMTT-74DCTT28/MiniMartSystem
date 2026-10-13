using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.API.Security;
using MiniMart.Shared.DTOs.Users;
using MiniMart.Shared.Entities;
using MiniMart.Shared.Enums;

namespace MiniMart.API.Controllers
{
    /// <summary>
    /// Quản lý tài khoản nhân viên (CRUD). Toàn bộ endpoint chỉ dành cho Admin
    /// vì đây là nghiệp vụ quản trị hệ thống.
    /// </summary>
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Tên claim chứa Id tài khoản, khớp với token do <see cref="JwtTokenService"/> phát hành.
        /// </summary>
        private const string SubjectClaimType = JwtRegisteredClaimNames.Sub;

        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Lấy danh sách toàn bộ tài khoản nhân viên (không trả về PasswordHash).</summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(user => user.Username)
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                    IsActive = user.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>Lấy thông tin một tài khoản theo Id.</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == id);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(ToDto(user));
        }

        /// <summary>Tạo tài khoản nhân viên mới. Mật khẩu được băm trước khi lưu.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var username = createUserDto.Username.Trim();

            // Username là duy nhất trong toàn hệ thống (xem AppDbContext.OnModelCreating).
            if (await _context.Users.AnyAsync(user => user.Username == username))
            {
                return Conflict(new { message = $"Tên đăng nhập '{username}' đã được sử dụng." });
            }

            var user = new User
            {
                Username = username,
                PasswordHash = PasswordHelper.Hash(createUserDto.Password),
                Role = createUserDto.Role,
                IsActive = createUserDto.IsActive
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToDto(user));
        }

        /// <summary>
        /// Cập nhật tài khoản: đổi tên đăng nhập, vai trò, trạng thái và (tùy chọn) mật khẩu.
        /// Bỏ trống mật khẩu nghĩa là giữ nguyên mật khẩu cũ.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            var username = updateUserDto.Username.Trim();

            if (await _context.Users.AnyAsync(other => other.Id != id && other.Username == username))
            {
                return Conflict(new { message = $"Tên đăng nhập '{username}' đã được sử dụng." });
            }

            // Hai trường hợp có thể khiến người đang đăng nhập tự khóa mình khỏi màn hình quản trị.
            var isLockedOut = !updateUserDto.IsActive || updateUserDto.Role != UserRole.Admin;

            if (isLockedOut && CurrentUserId == id)
            {
                return BadRequest(new { message = "Không thể tự vô hiệu hóa hoặc hạ quyền tài khoản đang đăng nhập." });
            }

            if (isLockedOut && await IsLastActiveAdminAsync(id))
            {
                return BadRequest(new { message = "Không thể vô hiệu hóa hoặc hạ quyền quản trị viên cuối cùng của hệ thống." });
            }

            user.Username = username;
            user.Role = updateUserDto.Role;
            user.IsActive = updateUserDto.IsActive;

            // Chỉ đổi mật khẩu khi client gửi mật khẩu mới.
            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                user.PasswordHash = PasswordHelper.Hash(updateUserDto.Password);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Xóa tài khoản nhân viên.</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            if (CurrentUserId == id)
            {
                return BadRequest(new { message = "Không thể xóa tài khoản đang đăng nhập." });
            }

            if (await IsLastActiveAdminAsync(id))
            {
                return BadRequest(new { message = "Không thể xóa quản trị viên cuối cùng của hệ thống." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Id tài khoản đang đăng nhập, lấy từ claim <c>sub</c> của access token.</summary>
        private Guid? CurrentUserId
        {
            get
            {
                var value = User.FindFirst(SubjectClaimType)?.Value;
                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        /// <summary>
        /// Kiểm tra tài khoản có phải là quản trị viên đang hoạt động cuối cùng của hệ thống hay không.
        /// Chỉ có ý nghĩa khi tài khoản đang được xét hiện là Admin.
        /// </summary>
        private async Task<bool> IsLastActiveAdminAsync(Guid userId)
        {
            var target = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == userId);

            if (target is null || target.Role != UserRole.Admin)
            {
                return false;
            }

            return !await _context.Users.AnyAsync(other =>
                other.Id != userId && other.Role == UserRole.Admin && other.IsActive);
        }

        private static UserDto ToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                IsActive = user.IsActive
            };
        }
    }
}
