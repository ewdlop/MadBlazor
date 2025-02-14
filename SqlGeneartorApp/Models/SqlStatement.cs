using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SqlGeneratorApp.Models;

[Table("SqlStatements")]
public class SqlStatement
{
    [Key]
    public int SqlStatementId { get; set; }

    public int DatabaseObjectId { get; set; }

    [Required]
    public SqlStatementType StatementType { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string SqlText { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public string CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    [MaxLength(500)]
    public string Parameters { get; set; }

    public int? Version { get; set; }

    // Navigation property
    public DatabaseObject DatabaseObject { get; set; }

    public ICollection<SqlStatementDependency> Dependencies { get; set; }
    public ICollection<SqlStatementDependency> DependentStatements { get; set; }
}
