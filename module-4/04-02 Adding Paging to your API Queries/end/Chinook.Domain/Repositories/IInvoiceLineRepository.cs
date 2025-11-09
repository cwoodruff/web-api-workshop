using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IInvoiceLineRepository : IDisposable
{
    Task<PagedList<InvoiceLine>> GetAll(int pageNumber, int pageSize);
    Task<InvoiceLine> GetById(int id);
    Task<PagedList<InvoiceLine>> GetByInvoiceId(int id, int pageNumber, int pageSize);
    Task<PagedList<InvoiceLine>> GetByTrackId(int id, int pageNumber, int pageSize);
    Task<InvoiceLine> Add(InvoiceLine newInvoiceLine);
    Task<bool> Update(InvoiceLine invoiceLine);
    Task<bool> Delete(int id);
}