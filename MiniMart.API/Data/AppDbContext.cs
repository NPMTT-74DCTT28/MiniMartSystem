using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Cấu hình ràng buộc các bảng (Fluent API) nếu cần thiết ở đây
        }
    }
}