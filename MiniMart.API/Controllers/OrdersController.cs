using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.Shared.DTOs.Orders;
using MiniMart.Shared.Entities;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private const decimal MaxAmount = 9999999999999999.99m;

        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(existingOrder => existingOrder.OrderDetails)
                .FirstOrDefaultAsync(existingOrder => existingOrder.Id == id);

            if (order is null)
            {
                return NotFound();
            }

            return Ok(ToDto(order));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = CurrentUserId;

            if (userId is null)
            {
                return Unauthorized();
            }

            var details = createOrderDto.Details;

            if (createOrderDto.CustomerId is <= 0)
            {
                return BadRequest("Khách hàng không hợp lệ.");
            }

            if (details is null || details.Count == 0)
            {
                return BadRequest("Hóa đơn phải có ít nhất một sản phẩm.");
            }

            if (details.Any(detail => detail is null || detail.ProductId <= 0 || detail.Quantity <= 0))
            {
                return BadRequest("Thông tin sản phẩm trong hóa đơn không hợp lệ.");
            }

            var quantitiesByProduct = new Dictionary<int, int>();

            try
            {
                foreach (var detail in details)
                {
                    quantitiesByProduct[detail.ProductId] = checked(
                        quantitiesByProduct.GetValueOrDefault(detail.ProductId) + detail.Quantity);
                }
            }
            catch (OverflowException)
            {
                return BadRequest("Tổng số lượng sản phẩm vượt quá giới hạn cho phép.");
            }

            var productIds = quantitiesByProduct.Keys.ToArray();

            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            if (!await _context.Users.AnyAsync(user => user.Id == userId.Value && user.IsActive))
            {
                return Unauthorized();
            }

            if (createOrderDto.CustomerId.HasValue
                && !await _context.Customers.AnyAsync(customer => customer.Id == createOrderDto.CustomerId.Value))
            {
                return BadRequest("Khách hàng không tồn tại.");
            }

            var products = await _context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);

            if (products.Count != productIds.Length)
            {
                return BadRequest("Một hoặc nhiều sản phẩm không tồn tại.");
            }

            if (products.Values.Any(product => product.StockQuantity < quantitiesByProduct[product.Id]))
            {
                return BadRequest("Không đủ số lượng tồn kho cho một hoặc nhiều sản phẩm.");
            }

            if (products.Values.Any(product => product.Price < 0 || product.Price > MaxAmount))
            {
                return BadRequest("Giá sản phẩm không hợp lệ.");
            }

            decimal totalAmount = 0;

            try
            {
                foreach (var detail in details)
                {
                    totalAmount = checked(totalAmount + products[detail.ProductId].Price * detail.Quantity);
                }
            }
            catch (OverflowException)
            {
                return BadRequest("Tổng tiền hóa đơn vượt quá giới hạn cho phép.");
            }

            if (totalAmount > MaxAmount)
            {
                return BadRequest("Tổng tiền hóa đơn vượt quá giới hạn cho phép.");
            }

            foreach (var (productId, quantity) in quantitiesByProduct)
            {
                products[productId].StockQuantity -= quantity;
            }

            var order = new Order
            {
                CustomerId = createOrderDto.CustomerId,
                UserId = userId.Value,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                OrderDetails = details.Select(detail => new OrderDetail
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    UnitPrice = products[detail.ProductId].Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ToDto(order));
        }

        private Guid? CurrentUserId
        {
            get
            {
                var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        private static OrderDto ToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Details = order.OrderDetails?.Select(detail => new OrderDetailDto
                {
                    Id = detail.Id,
                    OrderId = detail.OrderId,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice
                }).ToList() ?? new List<OrderDetailDto>()
            };
        }
    }
}
