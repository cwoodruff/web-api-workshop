using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chinook.Domain.Entities;
using Chinook.Domain.Extensions;

namespace Chinook.Domain.Repositories;

public interface IArtistRepository : IDisposable
{
    Task<PagedList<Artist>> GetAll(int pageNumber, int pageSize);
    Task<Artist> GetById(int id);
    Task<Artist> Add(Artist newArtist);
    Task<bool> Update(Artist artist);
    Task<bool> Delete(int id);
}