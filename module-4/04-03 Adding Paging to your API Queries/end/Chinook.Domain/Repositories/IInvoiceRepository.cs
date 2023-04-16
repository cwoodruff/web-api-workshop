using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IInvoiceRepository : IDisposable
{
    Task<PagedList<Invoice>> GetAll(int pageNumber, int pageSize);
    Task<Invoice> GetById(int id);
    Task<PagedList<Invoice>> GetByCustomerId(int id, int pageNumber, int pageSize);
    Task<Invoice> Add(Invoice newInvoice);
    Task<bool> Update(Invoice invoice);
    Task<bool> Delete(int id);
    Task<PagedList<Invoice>> GetByEmployeeId(int id, int pageNumber, int pageSize);
}