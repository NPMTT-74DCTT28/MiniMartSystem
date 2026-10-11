using Microsoft.EntityFrameworkCore;
using MiniMart.Shared.Entities;

namespace MiniMart.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Các thành viên tự khai báo DbSet<T> của mình dưới đây.
        // Ví dụ:
        // public DbSet<Product> Products { get; set; }
        // public DbSet<Customer> Customers { get; set; }

        // Member 1 (Leader) - Quản lý tài khoản nhân viên.
        public DbSet<User> Users { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Cấu hình ràng buộc các bảng (Fluent API) nếu cần thiết ở đây

            // Username phải là duy nhất trong toàn hệ thống.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}