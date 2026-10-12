using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.Shared.DTOs.Products;
using MiniMart.Shared.Entities;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Select(product => new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    ImageUrl = product.ImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(ToDto(product));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto)
        {
            if (!await _context.Categories.AnyAsync(category => category.Id == createProductDto.CategoryId))
            {
                return BadRequest("Danh mục không tồn tại.");
            }

            var product = new Product
            {
                Name = createProductDto.Name,
                CategoryId = createProductDto.CategoryId,
                Price = createProductDto.Price,
                StockQuantity = createProductDto.StockQuantity,
                ImageUrl = createProductDto.ImageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ToDto(product));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            if (!await _context.Categories.AnyAsync(category => category.Id == updateProductDto.CategoryId))
            {
                return BadRequest("Danh mục không tồn tại.");
            }

            product.Name = updateProductDto.Name;
            product.CategoryId = updateProductDto.CategoryId;
            product.Price = updateProductDto.Price;
            product.StockQuantity = updateProductDto.StockQuantity;
            product.ImageUrl = updateProductDto.ImageUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static ProductDto ToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl
            };
        }
    }
}
