using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.Shared.DTOs.Suppliers;
using MiniMart.Shared.Entities;
using MiniMart.Shared.Enums;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .OrderBy(supplier => supplier.Name)
                .Select(supplier => new SupplierDto
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    Phone = supplier.Phone,
                    Address = supplier.Address
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(supplier => supplier.Id == id);

            if (supplier is null)
            {
                return NotFound();
            }

            return Ok(ToDto(supplier));
        }

        [HttpPost]
        public async Task<IActionResult> CreateSupplier(CreateSupplierDto createSupplierDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supplier = new Supplier
            {
                Name = createSupplierDto.Name.Trim(),
                Phone = createSupplierDto.Phone.Trim(),
                Address = createSupplierDto.Address.Trim()
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, ToDto(supplier));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSupplier(int id, UpdateSupplierDto updateSupplierDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier is null)
            {
                return NotFound();
            }

            supplier.Name = updateSupplierDto.Name.Trim();
            supplier.Phone = updateSupplierDto.Phone.Trim();
            supplier.Address = updateSupplierDto.Address.Trim();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier is null)
            {
                return NotFound();
            }

            if (await _context.ImportReceipts.AnyAsync(receipt => receipt.SupplierId == id))
            {
                return Conflict(new { message = "Không thể xóa nhà cung cấp đã có phiếu nhập." });
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static SupplierDto ToDto(Supplier supplier)
        {
            return new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                Address = supplier.Address
            };
        }
    }
}
