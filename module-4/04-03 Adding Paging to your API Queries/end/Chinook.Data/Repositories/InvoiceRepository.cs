﻿using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using Chinook.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Data.Repositories;

/// <summary>
/// The invoice repository.
/// </summary>
public class InvoiceRepository : IInvoiceRepository
{
    /// <summary>
    /// The _context.
    /// </summary>
    private readonly ChinookContext _context;

    public InvoiceRepository(ChinookContext context)
    {
        _context = context;
    }

    private async Task<bool> InvoiceExists(int id) =>
        await _context.Invoices.AnyAsync(i => i.Id == id);

    public void Dispose() => _context.Dispose();

    // public List<Invoice> GetAll(int pageNumber, int pageSize) =>
    //     _context.Invoices.ToListAsync();

    public async Task<PagedList<Invoice>> GetAll(int pageNumber, int pageSize) => 
        await PagedList<Invoice>.ToPagedListAsync(_context.Set<Invoice>().AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<Invoice> GetById(int id) =>
        await _context.Invoices.FindAsync(id);

    public async Task<Invoice> Add(Invoice newInvoice)
    {
        await _context.Invoices.AddAsync(newInvoice);
        await _context.SaveChangesAsync();
        return newInvoice;
    }

    public async Task<bool> Update(Invoice invoice)
    {
        if (!await InvoiceExists(invoice.Id))
            return false;
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await InvoiceExists(id))
            return false;
        var toRemove = await _context.Invoices.FindAsync(id);
        _context.Invoices.Remove(toRemove);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedList<Invoice>> GetByEmployeeId(int id, int pageNumber, int pageSize) =>
        await PagedList<Invoice>.ToPagedListAsync(_context.Customers.Where(a => a.SupportRepId == id).SelectMany(t => t.Invoices)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<PagedList<Invoice>> GetByCustomerId(int id, int pageNumber, int pageSize) =>
        await PagedList<Invoice>.ToPagedListAsync(_context.Invoices.Where(a => a.CustomerId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
}