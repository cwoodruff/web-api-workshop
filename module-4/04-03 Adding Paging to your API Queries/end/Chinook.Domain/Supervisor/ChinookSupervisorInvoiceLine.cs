using Chinook.Domain.ApiModels;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<PagedList<InvoiceLineApiModel>> GetAllInvoiceLine(int pageNumber, int pageSize)
    {
        var invoiceLines = await _invoiceLineRepository.GetAll(pageNumber, pageSize);
        var invoiceLineApiModels = invoiceLines.ConvertAll();

        foreach (var invoiceLine in invoiceLineApiModels)
        {
            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("InvoiceLine-", invoiceLine.Id), invoiceLine, (TimeSpan)cacheEntryOptions);
        }
        var newPagedList = new PagedList<InvoiceLineApiModel>(invoiceLineApiModels.ToList(), invoiceLines.TotalCount, invoiceLines.CurrentPage, invoiceLines.PageSize);
        return newPagedList;
    }

    public async Task<InvoiceLineApiModel> GetInvoiceLineById(int id)
    {
        var invoiceLineApiModelCached = _cache.Get<InvoiceLineApiModel>(string.Concat("InvoiceLine-", id));

        if (invoiceLineApiModelCached != null)
        {
            return invoiceLineApiModelCached;
        }
        else
        {
            var invoiceLine = await _invoiceLineRepository.GetById(id);
            if (invoiceLine == null) return null!;
            var invoiceLineApiModel = invoiceLine.Convert();
            invoiceLineApiModel.Track = await GetTrackById(invoiceLineApiModel.TrackId);
            invoiceLineApiModel.Invoice = await GetInvoiceById(invoiceLineApiModel.InvoiceId);
            if (invoiceLineApiModel.Track != null) invoiceLineApiModel.TrackName = invoiceLineApiModel.Track.Name;

            var cacheEntryOptions =
                new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(604800))
                    .AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(604800);
            ;
            _cache.Set(string.Concat("InvoiceLine-", invoiceLineApiModel.Id), invoiceLineApiModel,
                (TimeSpan)cacheEntryOptions);

            return invoiceLineApiModel;
        }
    }

    public async Task<PagedList<InvoiceLineApiModel>> GetInvoiceLineByInvoiceId(int id, int pageNumber, int pageSize)
    {
        var invoiceLines = await _invoiceLineRepository.GetByInvoiceId(id, pageNumber, pageSize);
        var invoiceLineApiModels = invoiceLines.ConvertAll();
        var newPagedList = new PagedList<InvoiceLineApiModel>(invoiceLineApiModels.ToList(), invoiceLines.TotalCount, invoiceLines.CurrentPage, invoiceLines.PageSize);
        return newPagedList;
    }

    public async Task<PagedList<InvoiceLineApiModel>> GetInvoiceLineByTrackId(int id, int pageNumber, int pageSize)
    {
        var invoiceLines = await _invoiceLineRepository.GetByTrackId(id, pageNumber, pageSize);
        var invoiceLineApiModels = invoiceLines.ConvertAll();
        var newPagedList = new PagedList<InvoiceLineApiModel>(invoiceLineApiModels.ToList(), invoiceLines.TotalCount, invoiceLines.CurrentPage, invoiceLines.PageSize);
        return newPagedList;
    }

    public async Task<InvoiceLineApiModel> AddInvoiceLine(InvoiceLineApiModel newInvoiceLineApiModel)
    {
        await _invoiceLineValidator.ValidateAndThrowAsync(newInvoiceLineApiModel);

        var invoiceLine = newInvoiceLineApiModel.Convert();

        invoiceLine = await _invoiceLineRepository.Add(invoiceLine);
        newInvoiceLineApiModel.Id = invoiceLine.Id;
        return newInvoiceLineApiModel;
    }

    public async Task<bool> UpdateInvoiceLine(InvoiceLineApiModel invoiceLineApiModel)
    {
        await _invoiceLineValidator.ValidateAndThrowAsync(invoiceLineApiModel);

        var invoiceLine = await _invoiceLineRepository.GetById(invoiceLineApiModel.InvoiceId);

        if (invoiceLine == null) return false;
        invoiceLine.Id = invoiceLineApiModel.Id;
        invoiceLine.InvoiceId = invoiceLineApiModel.InvoiceId;
        invoiceLine.TrackId = invoiceLineApiModel.TrackId;
        invoiceLine.UnitPrice = invoiceLineApiModel.UnitPrice;
        invoiceLine.Quantity = invoiceLineApiModel.Quantity;

        return await _invoiceLineRepository.Update(invoiceLine);
    }

    public Task<bool> DeleteInvoiceLine(int id)
        => _invoiceLineRepository.Delete(id);
}