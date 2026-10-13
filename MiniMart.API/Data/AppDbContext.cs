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

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<ImportReceipt> ImportReceipts { get; set; }

        public DbSet<ImportReceiptDetail> ImportReceiptDetails { get; set; }
        // Order management
        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Cấu hình ràng buộc các bảng (Fluent API) nếu cần thiết ở đây

            // Username phải là duy nhất trong toàn hệ thống.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Role là enum UserRole nhưng vẫn lưu trong DB dạng chuỗi ("Admin" | "Staff")
            // để giữ nguyên cột nvarchar(20) và dữ liệu hiện có (không phát sinh migration).
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Order - OrderDetail relationship: One Order has many OrderDetails
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId);
        }
    }
}