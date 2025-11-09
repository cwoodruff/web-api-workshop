using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface ICustomerRepository : IDisposable
{
    Task<PagedList<Customer>> GetAll(int pageNumber, int pageSize);
    Task<Customer> GetById(int id);
    Task<PagedList<Customer>> GetBySupportRepId(int id, int pageNumber, int pageSize);
    Task<Customer> Add(Customer newCustomer);
    Task<bool> Update(Customer customer);
    Task<bool> Delete(int id);
}