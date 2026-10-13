using Microsoft.EntityFrameworkCore;
using MiniMart.API.Security;
using MiniMart.Shared.Entities;
using MiniMart.Shared.Enums;

namespace MiniMart.API.Data
{
    /// <summary>
    /// Tạo dữ liệu khởi tạo cần thiết để hệ thống dùng được ngay sau khi apply migration.
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>Tên đăng nhập của tài khoản quản trị mặc định.</summary>
        public const string DefaultAdminUsername = "admin";

        /// <summary>
        /// Mật khẩu của tài khoản quản trị mặc định. Chỉ dùng cho môi trường phát triển,
        /// phải đổi trước khi triển khai thật.
        /// </summary>
        public const string DefaultAdminPassword = "Admin@123";

        /// <summary>
        /// Tạo tài khoản Admin mặc định nếu chưa tồn tại, nhờ đó có thể đăng nhập ngay sau khi
        /// apply migration. Hàm an toàn khi gọi lặp lại nhiều lần.
        /// </summary>
        /// <param name="services">Service provider của ứng dụng, dùng để tạo scope lấy <see cref="AppDbContext"/>.</param>
        /// <param name="logger">Logger để ghi cảnh báo về tài khoản mặc định.</param>
        public static async Task SeedDefaultAdminAsync(IServiceProvider services, ILogger logger)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var exists = await context.Users
                .AsNoTracking()
                .AnyAsync(user => user.Username == DefaultAdminUsername);

            if (exists)
            {
                return;
            }

            context.Users.Add(new User
            {
                Username = DefaultAdminUsername,
                PasswordHash = PasswordHelper.Hash(DefaultAdminPassword),
                Role = UserRole.Admin,
                IsActive = true
            });

            await context.SaveChangesAsync();

            logger.LogWarning(
                "Đã tạo tài khoản quản trị mặc định '{Username}'. Hãy đổi mật khẩu trước khi triển khai thật.",
                DefaultAdminUsername);
        }
    }
}
