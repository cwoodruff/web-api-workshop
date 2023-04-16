using System.ComponentModel.DataAnnotations;

namespace Chinook.Domain.Identity;

public class LoginModel
{
    [Required] public string? Email { get; set; }
    [Required] public string? Password { get; set; }
}