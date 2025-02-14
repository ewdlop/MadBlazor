using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SqlGeneratorApp.Models;

[Table("SqlStatementVersions")]
public class SqlStatementVersion
{
    [Key]
    public int VersionId { get; set; }

    public int SqlStatementId { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string SqlText { get; set; }

    public int VersionNumber { get; set; }

    public string ChangedBy { get; set; }

    public DateTime ChangedDate { get; set; }

    [MaxLength(500)]
    public string ChangeDescription { get; set; }

    // Navigation property
    public SqlStatement SqlStatement { get; set; }
}
