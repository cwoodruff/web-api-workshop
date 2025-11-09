using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<EmployeeApiModel>> GetAllEmployee()
    {
        var key = _vCache.All("employee");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Employee> employees = await _employeeRepository.GetAll();
            return employees.ConvertAll();
        }, options) ?? Array.Empty<EmployeeApiModel>();
    }

    public async Task<EmployeeApiModel?> GetEmployeeById(int id)
    {
        var key = _vCache.EntityById("employee", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1))
            .SetSlidingExpiration(TimeSpan.FromHours(1));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var employee = await _employeeRepository.GetById(id);
            if (employee == null) return null;
            var employeeApiModel = employee.Convert();
            return employeeApiModel;
        }, options);
    }

    public async Task<EmployeeApiModel?> GetEmployeeReportsTo(int id)
    {
        var key = _vCache.ByFk("employee", "reports-to", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var employee = await _employeeRepository.GetReportsTo(id);
            return employee.Convert();
        }, options);
    }

    public async Task<EmployeeApiModel> AddEmployee(EmployeeApiModel newEmployeeApiModel)
    {
        await _employeeValidator.ValidateAndThrowAsync(newEmployeeApiModel);

        var employee = newEmployeeApiModel.Convert();

        employee = await _employeeRepository.Add(employee);
        newEmployeeApiModel.Id = employee.Id;

        _vCache.BumpVersion("employee");
        _vCache.BumpVersion("customer");
        return newEmployeeApiModel;
    }

    public async Task<bool> UpdateEmployee(EmployeeApiModel employeeApiModel)
    {
        await _employeeValidator.ValidateAndThrowAsync(employeeApiModel);

        var employee = await _employeeRepository.GetById(employeeApiModel.Id);

        if (employee == null) return false;
        employee.Id = employeeApiModel.Id;
        employee.LastName = employeeApiModel.LastName;
        employee.FirstName = employeeApiModel.FirstName;
        employee.Title = employeeApiModel.Title ?? string.Empty;
        employee.ReportsTo = employeeApiModel.ReportsTo;
        employee.BirthDate = employeeApiModel.BirthDate;
        employee.HireDate = employeeApiModel.HireDate;
        employee.Address = employeeApiModel.Address ?? string.Empty;
        employee.City = employeeApiModel.City ?? string.Empty;
        employee.State = employeeApiModel.State ?? string.Empty;
        employee.Country = employeeApiModel.Country ?? string.Empty;
        employee.PostalCode = employeeApiModel.PostalCode ?? string.Empty;
        employee.Phone = employeeApiModel.Phone ?? string.Empty;
        employee.Fax = employeeApiModel.Fax ?? string.Empty;
        employee.Email = employeeApiModel.Email ?? string.Empty;

        var updated = await _employeeRepository.Update(employee);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("employee", employee.Id));
            _vCache.BumpVersion("employee");
            _vCache.BumpVersion("customer");
        }
        return updated;
    }

    public async Task<bool> DeleteEmployee(int id)
    {
        var deleted = await _employeeRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("employee", id));
            _vCache.BumpVersion("employee");
            _vCache.BumpVersion("customer");
        }
        return deleted;
    }

    public async Task<IEnumerable<EmployeeApiModel>> GetEmployeeDirectReports(int id)
    {
        var key = _vCache.ByFk("employee", "direct-reports", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var employees = await _employeeRepository.GetDirectReports(id);
            return employees.ConvertAll();
        }, options) ?? Array.Empty<EmployeeApiModel>();
    }

    public async Task<IEnumerable<EmployeeApiModel>> GetDirectReports(int id)
    {
        var key = _vCache.ByFk("employee", "direct-reports", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var employees = await _employeeRepository.GetDirectReports(id);
            return employees.ConvertAll();
        }, options) ?? Array.Empty<EmployeeApiModel>();
    }
}