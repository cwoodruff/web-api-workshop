using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IMediaTypeRepository : IDisposable
{
    Task<PagedList<MediaType>> GetAll(int pageNumber, int pageSize);
    Task<MediaType> GetById(int id);
    Task<MediaType> Add(MediaType newMediaType);
    Task<bool> Update(MediaType mediaType);
    Task<bool> Delete(int id);
}