using Chinook.Domain.Helpers;

namespace Chinook.Domain.ApiModels;

public abstract class BaseApiModel : Representation
{
    public int Id { get; set; }
}
