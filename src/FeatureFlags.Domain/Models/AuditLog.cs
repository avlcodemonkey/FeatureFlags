using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Models;

[Table("AuditLog")]
public class AuditLog {
    [Key]
    public long Id { get; set; }

    [Required]
    public Guid BatchId { get; set; }

    [Required]
    public string Entity { get; set; } = null!;

    [Required]
    public long PrimaryKey { get; set; }

    /// <summary>
    /// Should only be one of: Deleted = 2, Modified = 3, Added = 4
    /// </summary>
    [Required]
    public EntityState State { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public int? UserId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
