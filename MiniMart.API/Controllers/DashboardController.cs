using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.Shared.DTOs.Dashboard;
using MiniMart.Shared.Entities;

namespace MiniMart.API.Controllers
{
    /// <summary>
    /// Tổng hợp số liệu thống kê cho màn hình Dashboard từ các bảng do nhiều thành viên
    /// phụ trách (<c>Orders</c>, <c>OrderDetails</c>, <c>Products</c>, <c>Categories</c>,
    /// <c>Customers</c>, <c>Users</c>, <c>ImportReceipts</c>).
    /// </summary>
    /// <remarks>
    /// Client WinForms không được đọc trực tiếp database, nên toàn bộ số liệu được trả về
    /// trong một lần gọi duy nhất để màn hình chỉ cần một request.
    /// Endpoint chỉ tổng hợp dữ liệu, không ghi hay sửa dữ liệu của module nào.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        /// <summary>Số ngày mặc định của kỳ thống kê khi Client không truyền tham số.</summary>
        public const int DefaultRangeInDays = 30;

        /// <summary>Độ dài tối đa của kỳ thống kê, tránh biểu đồ quá dày và truy vấn quá lớn.</summary>
        public const int MaxRangeInDays = 366;

        /// <summary>Số dòng tối đa của các bảng xếp hạng (sản phẩm, nhân viên, khách hàng).</summary>
        public const int TopItemCount = 5;

        /// <summary>Ngưỡng tồn kho được coi là thấp, dùng cho thẻ cảnh báo nhập hàng.</summary>
        public const int LowStockThreshold = 10;

        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy số liệu thống kê của kỳ <paramref name="from"/> - <paramref name="to"/> (tính theo UTC).
        /// Bỏ trống tham số thì mặc định lấy 30 ngày gần nhất tính đến hôm nay.
        /// </summary>
        /// <param name="from">Ngày bắt đầu kỳ thống kê (UTC), mặc định là 29 ngày trước <paramref name="to"/>.</param>
        /// <param name="to">Ngày kết thúc kỳ thống kê (UTC), tính trọn ngày; mặc định là hôm nay.</param>
        [HttpGet]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var today = DateTime.UtcNow.Date;
            var toDate = (to ?? today).Date;
            var fromDate = (from ?? toDate.AddDays(-(DefaultRangeInDays - 1))).Date;

            if (fromDate > toDate)
            {
                return BadRequest(new { message = "Ngày bắt đầu không được sau ngày kết thúc." });
            }

            if ((toDate - fromDate).TotalDays >= MaxRangeInDays)
            {
                return BadRequest(new { message = $"Kỳ thống kê tối đa {MaxRangeInDays} ngày." });
            }

            // Khoảng nửa mở [rangeStart, rangeEnd) để lấy trọn ngày cuối cùng.
            var rangeStart = fromDate;
            var rangeEnd = toDate.AddDays(1);

            var ordersInRange = _context.Orders
                .AsNoTracking()
                .Where(order => order.OrderDate >= rangeStart && order.OrderDate < rangeEnd);

            var orderTotals = await ordersInRange
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    OrderCount = group.Count(),
                    Revenue = group.Sum(order => order.TotalAmount)
                })
                .FirstOrDefaultAsync();

            var orderCount = orderTotals?.OrderCount ?? 0;
            var totalRevenue = orderTotals?.Revenue ?? 0m;

            var importTotals = await _context.ImportReceipts
                .AsNoTracking()
                .Where(receipt => receipt.ImportDate >= rangeStart && receipt.ImportDate < rangeEnd)
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    ReceiptCount = group.Count(),
                    Amount = group.Sum(receipt => receipt.TotalAmount)
                })
                .FirstOrDefaultAsync();

            var totalQuantitySold = await _context.OrderDetails
                .AsNoTracking()
                .Where(detail => detail.Order.OrderDate >= rangeStart && detail.Order.OrderDate < rangeEnd)
                .SumAsync(detail => (int?)detail.Quantity) ?? 0;

            var summary = new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                AverageOrderValue = orderCount == 0 ? 0m : totalRevenue / orderCount,
                TotalImportAmount = importTotals?.Amount ?? 0m,
                ImportReceiptCount = importTotals?.ReceiptCount ?? 0,
                TotalProducts = await _context.Products.AsNoTracking().CountAsync(),
                LowStockProductCount = await _context.Products
                    .AsNoTracking()
                    .CountAsync(product => product.StockQuantity <= LowStockThreshold),
                TotalCustomers = await _context.Customers.AsNoTracking().CountAsync(),
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(),
                TotalQuantitySold = totalQuantitySold
            };

            return Ok(new DashboardStatisticsDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                Summary = summary,
                RevenueByDay = await BuildRevenueByDayAsync(ordersInRange, fromDate, toDate),
                TopProducts = await BuildTopProductsAsync(rangeStart, rangeEnd),
                RevenueByCategory = await BuildRevenueByCategoryAsync(rangeStart, rangeEnd),
                RevenueByUser = await BuildRevenueByUserAsync(ordersInRange),
                TopCustomers = await BuildTopCustomersAsync(ordersInRange)
            });
        }

        /// <summary>
        /// Doanh thu từng ngày trong kỳ. Những ngày không có hóa đơn vẫn được trả về với giá trị 0
        /// để biểu đồ của Client không bị đứt quãng.
        /// </summary>
        private static async Task<List<RevenueByDayDto>> BuildRevenueByDayAsync(
            IQueryable<Order> ordersInRange,
            DateTime fromDate,
            DateTime toDate)
        {
            var rows = await ordersInRange
                .GroupBy(order => order.OrderDate.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    OrderCount = group.Count(),
                    Revenue = group.Sum(order => order.TotalAmount)
                })
                .ToListAsync();

            var byDate = rows.ToDictionary(row => row.Date, row => row);
            var result = new List<RevenueByDayDto>();

            for (var day = fromDate; day <= toDate; day = day.AddDays(1))
            {
                if (byDate.TryGetValue(day, out var row))
                {
                    result.Add(new RevenueByDayDto
                    {
                        Date = day,
                        OrderCount = row.OrderCount,
                        Revenue = row.Revenue
                    });
                }
                else
                {
                    result.Add(new RevenueByDayDto { Date = day });
                }
            }

            return result;
        }

        /// <summary>Sản phẩm bán chạy nhất trong kỳ, xếp theo doanh thu rồi tới số lượng bán.</summary>
        private async Task<List<TopProductDto>> BuildTopProductsAsync(DateTime rangeStart, DateTime rangeEnd)
        {
            return await _context.OrderDetails
                .AsNoTracking()
                .Where(detail => detail.Order.OrderDate >= rangeStart && detail.Order.OrderDate < rangeEnd)
                .GroupBy(detail => new { detail.ProductId, detail.Product.Name })
                .Select(group => new TopProductDto
                {
                    ProductId = group.Key.ProductId,
                    ProductName = group.Key.Name,
                    QuantitySold = group.Sum(detail => detail.Quantity),
                    Revenue = group.Sum(detail => detail.Quantity * detail.UnitPrice)
                })
                .OrderByDescending(product => product.Revenue)
                .ThenByDescending(product => product.QuantitySold)
                .Take(TopItemCount)
                .ToListAsync();
        }

        /// <summary>Doanh thu chia theo danh mục sản phẩm trong kỳ.</summary>
        private async Task<List<RevenueByCategoryDto>> BuildRevenueByCategoryAsync(DateTime rangeStart, DateTime rangeEnd)
        {
            return await _context.OrderDetails
                .AsNoTracking()
                .Where(detail => detail.Order.OrderDate >= rangeStart && detail.Order.OrderDate < rangeEnd)
                .GroupBy(detail => new { detail.Product.CategoryId, detail.Product.Category.Name })
                .Select(group => new RevenueByCategoryDto
                {
                    CategoryId = group.Key.CategoryId,
                    CategoryName = group.Key.Name,
                    QuantitySold = group.Sum(detail => detail.Quantity),
                    Revenue = group.Sum(detail => detail.Quantity * detail.UnitPrice)
                })
                .OrderByDescending(category => category.Revenue)
                .ToListAsync();
        }

        /// <summary>Doanh thu chia theo nhân viên bán hàng trong kỳ.</summary>
        private async Task<List<RevenueByUserDto>> BuildRevenueByUserAsync(
            IQueryable<Order> ordersInRange)
        {
            return await ordersInRange
                .GroupBy(order => new { order.UserId, order.User.Username })
                .Select(group => new RevenueByUserDto
                {
                    UserId = group.Key.UserId,
                    Username = group.Key.Username,
                    OrderCount = group.Count(),
                    Revenue = group.Sum(order => order.TotalAmount)
                })
                .OrderByDescending(user => user.Revenue)
                .Take(TopItemCount)
                .ToListAsync();
        }

        /// <summary>Khách hàng chi tiêu nhiều nhất trong kỳ (không tính hóa đơn khách lẻ).</summary>
        private async Task<List<TopCustomerDto>> BuildTopCustomersAsync(
            IQueryable<Order> ordersInRange)
        {
            var totals = await ordersInRange
                .Where(order => order.CustomerId != null)
                .GroupBy(order => order.CustomerId!.Value)
                .Select(group => new
                {
                    CustomerId = group.Key,
                    OrderCount = group.Count(),
                    Revenue = group.Sum(order => order.TotalAmount)
                })
                .OrderByDescending(item => item.Revenue)
                .Take(TopItemCount)
                .ToListAsync();

            if (totals.Count == 0)
            {
                return new List<TopCustomerDto>();
            }

            var customerIds = totals.Select(item => item.CustomerId).ToList();

            var customers = await _context.Customers
                .AsNoTracking()
                .Where(customer => customerIds.Contains(customer.Id))
                .Select(customer => new
                {
                    customer.Id,
                    customer.FullName,
                    customer.PhoneNumber,
                    customer.TotalPoints
                })
                .ToDictionaryAsync(customer => customer.Id);

            var result = new List<TopCustomerDto>();

            foreach (var item in totals)
            {
                if (!customers.TryGetValue(item.CustomerId, out var customer))
                {
                    // Khách hàng đã bị xóa nhưng hóa đơn cũ vẫn còn (CustomerId được đặt null khi xóa).
                    continue;
                }

                result.Add(new TopCustomerDto
                {
                    CustomerId = item.CustomerId,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    TotalPoints = customer.TotalPoints,
                    OrderCount = item.OrderCount,
                    Revenue = item.Revenue
                });
            }

            return result;
        }
    }
}
