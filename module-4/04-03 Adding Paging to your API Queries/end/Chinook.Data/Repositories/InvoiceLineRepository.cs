﻿using Chinook.Data.Data;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using Chinook.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Data.Repositories;

public class InvoiceLineRepository : IInvoiceLineRepository
{
    private readonly ChinookContext _context;

    public InvoiceLineRepository(ChinookContext context)
    {
        _context = context;
    }

    private async Task<bool> InvoiceLineExists(int id) =>
        await _context.InvoiceLines.AnyAsync(i => i.Id == id);

    public void Dispose() => _context.Dispose();

    public async Task<PagedList<InvoiceLine>> GetAll(int pageNumber, int pageSize) =>
        await PagedList<InvoiceLine>.ToPagedListAsync(_context.Set<InvoiceLine>().AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<InvoiceLine> GetById(int id) =>
        await _context.InvoiceLines.FindAsync(id);

    public async Task<InvoiceLine> Add(InvoiceLine newInvoiceLine)
    {
        await _context.InvoiceLines.AddAsync(newInvoiceLine);
        await _context.SaveChangesAsync();
        return newInvoiceLine;
    }

    public async Task<bool> Update(InvoiceLine invoiceLine)
    {
        if (!await InvoiceLineExists(invoiceLine.Id))
            return false;
        _context.InvoiceLines.Update(invoiceLine);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await InvoiceLineExists(id))
            return false;
        var toRemove = await _context.InvoiceLines.FindAsync(id);
        _context.InvoiceLines.Remove(toRemove);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedList<InvoiceLine>> GetByInvoiceId(int id, int pageNumber, int pageSize) =>
        await PagedList<InvoiceLine>.ToPagedListAsync(_context.InvoiceLines.Where(a => a.InvoiceId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);

    public async Task<PagedList<InvoiceLine>> GetByTrackId(int id, int pageNumber, int pageSize) =>
        await PagedList<InvoiceLine>.ToPagedListAsync(_context.InvoiceLines.Where(a => a.TrackId == id)
                .AsNoTrackingWithIdentityResolution(),
            pageNumber,
            pageSize);
}