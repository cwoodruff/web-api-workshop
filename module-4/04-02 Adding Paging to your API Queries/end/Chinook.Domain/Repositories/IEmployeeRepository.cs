using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IEmployeeRepository : IDisposable
{
    Task<PagedList<Employee>> GetAll(int pageNumber, int pageSize);
    Task<Employee> GetById(int id);
    Task<Employee> GetReportsTo(int id);
    Task<Employee> Add(Employee newEmployee);
    Task<bool> Update(Employee employee);
    Task<bool> Delete(int id);
    Task<List<Employee>> GetDirectReports(int id);
}