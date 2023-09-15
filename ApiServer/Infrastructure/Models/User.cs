using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Models;

public class User
{
    [Key]
    [StringLength(20, MinimumLength = 5)]
    public string Id { get; set; }
    [StringLength(40, MinimumLength = 1)]
    public string Name { get; set; }
    [StringLength(256), NotNull]
    public string Password { get; set; }
    public bool UseFlag { get; set; } = false;
    public virtual List<Login> Logins { get; } = new();
}
