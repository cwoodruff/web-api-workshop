using Chinook.Domain.ApiModels;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;

namespace Chinook.Domain.Supervisor;

public partial class ChinookSupervisor
{
    public async Task<IEnumerable<CustomerApiModel>> GetAllCustomer()
    {
        var key = _vCache.All("customer");
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(3));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            List<Customer> customers = await _customerRepository.GetAll();
            return customers.ConvertAll();
        }, options) ?? Array.Empty<CustomerApiModel>();
    }

    public async Task<CustomerApiModel> GetCustomerById(int id)
    {
        var key = _vCache.EntityById("customer", id);
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1))
            .SetSlidingExpiration(TimeSpan.FromHours(1));

        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var customer = await _customerRepository.GetById(id);
            if (customer == null) return null!;
            var customerApiModel = customer.Convert();
            customerApiModel.Invoices = (await GetInvoiceByCustomerId(customerApiModel.Id)).ToList();
            customerApiModel.SupportRep = await GetEmployeeById(customerApiModel.SupportRepId);
            if (customerApiModel.SupportRep != null)
                customerApiModel.SupportRepName =
                    $"{customerApiModel.SupportRep.LastName}, {customerApiModel.SupportRep.FirstName}";
            return customerApiModel;
        }, options)!;
    }

    public async Task<IEnumerable<CustomerApiModel>> GetCustomerBySupportRepId(int id)
    {
        var key = _vCache.ByFk("customer", "by-supportrep", id);
        var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        return await _vCache.GetOrCreateAsync(key, async () =>
        {
            var customers = await _customerRepository.GetBySupportRepId(id);
            return customers.ConvertAll();
        }, options) ?? Array.Empty<CustomerApiModel>();
    }

    public async Task<CustomerApiModel> AddCustomer(CustomerApiModel newCustomerApiModel)
    {
        await _customerValidator.ValidateAndThrowAsync(newCustomerApiModel);

        var customer = newCustomerApiModel.Convert();

        customer = await _customerRepository.Add(customer);
        newCustomerApiModel.Id = customer.Id;

        _vCache.BumpVersion("customer");
        _vCache.BumpVersion("invoice");
        return newCustomerApiModel;
    }

    public async Task<bool> UpdateCustomer(CustomerApiModel customerApiModel)
    {
        await _customerValidator.ValidateAndThrowAsync(customerApiModel);

        var customer = await _customerRepository.GetById(customerApiModel.Id);

        if (customer == null) return false;
        customer.FirstName = customerApiModel.FirstName;
        customer.LastName = customerApiModel.LastName;
        customer.Company = customerApiModel.Company ?? string.Empty;
        customer.Address = customerApiModel.Address ?? string.Empty;
        customer.City = customerApiModel.City ?? string.Empty;
        customer.State = customerApiModel.State ?? string.Empty;
        customer.Country = customerApiModel.Country ?? string.Empty;
        customer.PostalCode = customerApiModel.PostalCode ?? string.Empty;
        customer.Phone = customerApiModel.Phone ?? string.Empty;
        customer.Fax = customerApiModel.Fax ?? string.Empty;
        customer.Email = customerApiModel.Email ?? string.Empty;
        customer.SupportRepId = customerApiModel.SupportRepId;

        var updated = await _customerRepository.Update(customer);
        if (updated)
        {
            _vCache.Remove(_vCache.EntityById("customer", customer.Id));
            _vCache.BumpVersion("customer");
            _vCache.BumpVersion("invoice");
        }
        return updated;
    }

    public async Task<bool> DeleteCustomer(int id)
    {
        var deleted = await _customerRepository.Delete(id);
        if (deleted)
        {
            _vCache.Remove(_vCache.EntityById("customer", id));
            _vCache.BumpVersion("customer");
        }
        return deleted;
    }
}