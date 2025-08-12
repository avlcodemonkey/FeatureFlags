using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Models;

[Table("UserToken")]
[Index(nameof(UserId), IsUnique = true)]
public class UserToken {
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Token user will enter to verify their identity.
    /// </summary>
    [StringLength(100)]
    public string Token { get; set; } = "";

    /// <summary>
    /// Second hidden token to make man-in-the-middle attacks harder.
    /// </summary>
    [StringLength(100)]
    public string HiddenToken { get; set; } = "";

    public DateTime? ExpirationDate { get; set; }

    public int Attempts { get; set; }

    public User User { get; set; } = null!;
}
