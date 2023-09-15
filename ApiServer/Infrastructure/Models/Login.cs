using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Models;

public class Login
{
    [Key]
    public long Id { get; set; }
    [StringLength(1024, MinimumLength = 100)]
    public string AccessToken { get; set; }
    [StringLength(40, MinimumLength = 32)]
    public string RefreshToken { get; set; }
    [NotNull]
    public DateTime? RefreshTokenExpired { get; set; } = DateTime.UtcNow;
    [NotNull]
    public DateTime LoginTime { get; set; } = DateTime.UtcNow;
    public bool UseFlag { get; set; } = false;
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    public DateTime LastLoginTime { get; set; } = DateTime.UtcNow;
}
