using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeatureFlags.Domain.Attributes;

namespace FeatureFlags.Domain.Models;

[Table("Language")]
public class Language : IAuditedEntity {
    [Key]
    public int Id { get; set; }

    [StringLength(10)]
    [Required]
    public string CountryCode { get; set; } = null!;

    public bool IsDefault { get; set; }

    [StringLength(10)]
    [Required]
    public string LanguageCode { get; set; } = null!;

    [StringLength(100)]
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// When language was created.
    /// </summary>
    /// <remarks>
    /// SQLite specific default value will need to change if backing database is changed.
    /// </remarks>
    [DefaultValueSql("(current_timestamp)")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// When language was updated.
    /// </summary>
    [Timestamp, DefaultValueSql("(current_timestamp)")]
    public DateTime UpdatedDate { get; set; }

    [NotMapped]
    public int TemporaryId { get; set; }
}
