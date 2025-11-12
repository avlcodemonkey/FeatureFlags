using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Models;

[Table("ApiRequest")]
[Index(nameof(ApiKeyId))]
public class ApiRequest {
    [Key]
    public int Id { get; set; }

    [Required]
    public int ApiKeyId { get; set; }

    [StringLength(50)]
    public string IpAddress { get; set; } = null!;

    public ApiKey ApiKey { get; set; } = null!;

    /// <summary>
    /// When request was made.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime RequestedDate { get; set; }
}
