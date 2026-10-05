using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.DTOs;
using MiniBank.Api.Models;

namespace MiniBank.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(MiniBankDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("FirstName, LastName and Email are required.");

        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Customers.AnyAsync(x => x.Email == email))
            return BadRequest("A customer with this email already exists.");

        var customer = new Customer
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = customer.Id },
            new CustomerResponse(customer.Id, customer.FirstName, customer.LastName,
                customer.Email, customer.CreatedAt, []));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAll()
    {
        var customers = await db.Customers
            .AsNoTracking()
            .Include(x => x.Accounts)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(customers.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(int id)
    {
        var customer = await db.Customers
            .AsNoTracking()
            .Include(x => x.Accounts)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (customer is null)
            return NotFound("Customer not found.");

        return Ok(ToResponse(customer));
    }

    private static CustomerResponse ToResponse(Customer customer) =>
        new(customer.Id, customer.FirstName, customer.LastName, customer.Email,
            customer.CreatedAt,
            customer.Accounts
                .OrderBy(x => x.Id)
                .Select(x => new AccountSummaryResponse(x.Id, x.AccountNumber, x.Balance))
                .ToList());
}
