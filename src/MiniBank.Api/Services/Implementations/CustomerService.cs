using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Exceptions;
using MiniBank.Api.Models.DTOs.Account;
using MiniBank.Api.Models.DTOs.Customer;
using MiniBank.Api.Models.Entities;
using MiniBank.Api.Services.Interfaces;

namespace MiniBank.Api.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly MiniBankDbContext _context;

    public CustomerService(MiniBankDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var emailNormalized = request.Email.Trim().ToLower();
        var emailExists = await _context.Customers.AnyAsync(c => c.Email.ToLower() == emailNormalized);

        if (emailExists)
        {
            throw new BusinessRuleException($"A customer with email '{request.Email}' already exists.");
        }

        var customer = new Customer
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = emailNormalized,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerResponse
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CustomerResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CustomerWithAccountsResponse> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {id} was not found.");
        }

        return new CustomerWithAccountsResponse
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt,
            Accounts = customer.Accounts.Select(a => new AccountResponse
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                CustomerId = a.CustomerId,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }
}
