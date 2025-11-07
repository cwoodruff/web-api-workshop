using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<InvoiceApiModel>> GetAllInvoice()
    {
        var key = _vCache.All("invoice");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(3));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Invoice> invoices = await _invoiceRepository.GetAll();
            return invoices.ConvertAll();
        }, options) ?? Array.Empty<InvoiceApiModel>();
    }

    public async Task<InvoiceApiModel?> GetInvoiceById(int id)
    {
        var key = _vCache.EntityById("invoice", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1))
            .SetSlidingExpiration(TimeSpan.FromHours(1));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var invoice = await _invoiceRepository.GetById(id);
            if (invoice == null) return null;
            var invoiceApiModel = invoice.Convert();
            invoiceApiModel.InvoiceLines = (await GetInvoiceLineByInvoiceId(invoiceApiModel.Id)).ToList();
            return invoiceApiModel;
        }, options);
    }

    public async Task<IEnumerable<InvoiceApiModel>> GetInvoiceByCustomerId(int id)
    {
        var key = _vCache.ByFk("invoice", "by-customer", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var invoices = await _invoiceRepository.GetByCustomerId(id);
            return invoices.ConvertAll();
        }, options) ?? Array.Empty<InvoiceApiModel>();
    }

    public async Task<InvoiceApiModel> AddInvoice(InvoiceApiModel newInvoiceApiModel)
    {
        await _invoiceValidator.ValidateAndThrowAsync(newInvoiceApiModel);

        var invoice = newInvoiceApiModel.Convert();

        invoice = await _invoiceRepository.Add(invoice);
        newInvoiceApiModel.Id = invoice.Id;

        _vCache.BumpVersion("invoice");
        return newInvoiceApiModel;
    }

    public async Task<bool> UpdateInvoice(InvoiceApiModel invoiceApiModel)
    {
        await _invoiceValidator.ValidateAndThrowAsync(invoiceApiModel);

        var invoice = await _invoiceRepository.GetById(invoiceApiModel.Id);

        if (invoice == null) return false;
        invoice.Id = invoiceApiModel.Id;
        invoice.CustomerId = invoiceApiModel.CustomerId;
        invoice.InvoiceDate = invoiceApiModel.InvoiceDate;
        invoice.BillingAddress = invoiceApiModel.BillingAddress ?? string.Empty;
        invoice.BillingCity = invoiceApiModel.BillingCity ?? string.Empty;
        invoice.BillingState = invoiceApiModel.BillingState ?? string.Empty;
        invoice.BillingCountry = invoiceApiModel.BillingCountry ?? string.Empty;
        invoice.BillingPostalCode = invoiceApiModel.BillingPostalCode ?? string.Empty;
        invoice.Total = invoiceApiModel.Total;

        var updated = await _invoiceRepository.Update(invoice);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("invoice", invoice.Id));
            _vCache.BumpVersion("invoice");
        }
        return updated;
    }

    public async Task<bool> DeleteInvoice(int id)
    {
        var deleted = await _invoiceRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("invoice", id));
            _vCache.BumpVersion("invoice");
        }
        return deleted;
    }


    public async Task<IEnumerable<InvoiceApiModel>> GetInvoiceByEmployeeId(int id)
    {
        var key = _vCache.ByFk("invoice", "by-employee", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var invoices = await _invoiceRepository.GetByEmployeeId(id);
            return invoices.ConvertAll();
        }, options) ?? Array.Empty<InvoiceApiModel>();
    }
}