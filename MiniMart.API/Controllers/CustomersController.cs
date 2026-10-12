using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.API.Data;
using MiniMart.Shared.DTOs.Customer;
using MiniMart.Shared.Entities;

namespace MiniMart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .Select(customer => new CustomerDto
                {
                    Id = customer.Id,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    TotalPoints = customer.TotalPoints
                })
                .ToListAsync();

            return Ok(customers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(customer => customer.Id == id);

            if (customer is null)
            {
                return NotFound();
            }

            return Ok(ToDto(customer));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = new Customer
            {
                FullName = createCustomerDto.FullName,
                PhoneNumber = createCustomerDto.PhoneNumber
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, ToDto(customer));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers.FindAsync(id);

            if (customer is null)
            {
                return NotFound();
            }

            customer.FullName = updateCustomerDto.FullName;
            customer.PhoneNumber = updateCustomerDto.PhoneNumber;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static CustomerDto ToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                TotalPoints = customer.TotalPoints
            };
        }
    }
}
