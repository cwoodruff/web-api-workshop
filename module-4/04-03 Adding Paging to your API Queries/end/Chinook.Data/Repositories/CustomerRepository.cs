﻿using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using Chinook.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ChinookContext _context;

    public CustomerRepository(ChinookContext context)
    {
        _context = context;
    }

    private async Task<bool> CustomerExists(int id) =>
        await _context.Customers.AnyAsync(c => c.Id == id);

    public void Dispose() => _context.Dispose();

    public async Task<PagedList<Customer>> GetAll(int pageNumber, int pageSize) =>
        await PagedList<Customer>.ToPagedListAsync(_context.Set<Customer>().AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<Customer> GetById(int id) =>
        await _context.Customers.FindAsync(id);

    public async Task<Customer> Add(Customer newCustomer)
    {
        await _context.Customers.AddAsync(newCustomer);
        await _context.SaveChangesAsync();
        return newCustomer;
    }

    public async Task<bool> Update(Customer customer)
    {
        if (!await CustomerExists(customer.Id))
            return false;
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await CustomerExists(id))
            return false;
        var toRemove = await _context.Customers.FindAsync(id);
        _context.Customers.Remove(toRemove);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedList<Customer>> GetBySupportRepId(int id, int pageNumber, int pageSize) =>
        await PagedList<Customer>.ToPagedListAsync(_context.Customers.Where(a => a.SupportRepId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
}