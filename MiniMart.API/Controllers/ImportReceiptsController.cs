using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.Shared.DTOs.ImportReceipts;
using MiniMart.Shared.Entities;
using MiniMart.Shared.Enums;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Route("api/[controller]")]
    public class ImportReceiptsController : ControllerBase
    {
        private const decimal MaxAmount = 9999999999999999.99m;

        private readonly AppDbContext _context;

        public ImportReceiptsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetImportReceipt(int id)
        {
            var receipt = await _context.ImportReceipts
                .AsNoTracking()
                .Include(importReceipt => importReceipt.Details)
                .FirstOrDefaultAsync(importReceipt => importReceipt.Id == id);

            if (receipt is null)
            {
                return NotFound();
            }

            return Ok(ToDto(receipt));
        }

        [HttpPost]
        public async Task<IActionResult> CreateImportReceipt(CreateImportReceiptDto createImportReceiptDto)
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

            var details = createImportReceiptDto.Details;

            if (createImportReceiptDto.SupplierId <= 0)
            {
                return BadRequest("Nhà cung cấp không hợp lệ.");
            }

            if (details is null || details.Count == 0)
            {
                return BadRequest("Phiếu nhập phải có ít nhất một sản phẩm.");
            }

            if (details.Any(detail => detail is null
                || detail.ProductId <= 0
                || detail.Quantity <= 0
                || detail.ImportPrice < 0
                || detail.ImportPrice > MaxAmount))
            {
                return BadRequest("Thông tin sản phẩm nhập kho không hợp lệ.");
            }

            decimal totalAmount = 0;

            try
            {
                foreach (var detail in details)
                {
                    totalAmount = checked(totalAmount + detail.Quantity * detail.ImportPrice);
                }
            }
            catch (OverflowException)
            {
                return BadRequest("Tổng tiền phiếu nhập vượt quá giới hạn cho phép.");
            }

            if (totalAmount > MaxAmount)
            {
                return BadRequest("Tổng tiền phiếu nhập vượt quá giới hạn cho phép.");
            }

            var productIds = details
                .Select(detail => detail.ProductId)
                .Distinct()
                .ToArray();

            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            if (!await _context.Suppliers.AnyAsync(supplier => supplier.Id == createImportReceiptDto.SupplierId))
            {
                return BadRequest("Nhà cung cấp không tồn tại.");
            }

            var products = await _context.Products
                .Where(product => productIds.Contains(product.Id))
                .ToDictionaryAsync(product => product.Id);

            if (products.Count != productIds.Length)
            {
                return BadRequest("Một hoặc nhiều sản phẩm không tồn tại.");
            }

            try
            {
                foreach (var detail in details)
                {
                    var product = products[detail.ProductId];
                    product.StockQuantity = checked(product.StockQuantity + detail.Quantity);
                }
            }
            catch (OverflowException)
            {
                return BadRequest("Số lượng tồn kho vượt quá giới hạn cho phép.");
            }

            var receipt = new ImportReceipt
            {
                SupplierId = createImportReceiptDto.SupplierId,
                UserId = userId.Value,
                ImportDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Details = details.Select(detail => new ImportReceiptDetail
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    ImportPrice = detail.ImportPrice
                }).ToList()
            };

            _context.ImportReceipts.Add(receipt);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetImportReceipt), new { id = receipt.Id }, ToDto(receipt));
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

        private static ImportReceiptDto ToDto(ImportReceipt receipt)
        {
            return new ImportReceiptDto
            {
                Id = receipt.Id,
                SupplierId = receipt.SupplierId,
                UserId = receipt.UserId,
                ImportDate = receipt.ImportDate,
                TotalAmount = receipt.TotalAmount,
                Details = receipt.Details?.Select(detail => new ImportReceiptDetailDto
                {
                    Id = detail.Id,
                    ImportReceiptId = detail.ImportReceiptId,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    ImportPrice = detail.ImportPrice
                }).ToList() ?? new List<ImportReceiptDetailDto>()
            };
        }
    }
}
