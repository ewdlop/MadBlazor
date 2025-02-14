using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SqlGeneratorApp.Models;

[Table("SqlStatementDependencies")]
public class SqlStatementDependency
{
    [Key]
    public int DependencyId { get; set; }

    public int ParentStatementId { get; set; }

    public int DependentStatementId { get; set; }

    [MaxLength(500)]
    public string DependencyType { get; set; }

    public DateTime CreatedDate { get; set; }

    // Navigation properties
    public SqlStatement ParentStatement { get; set; }
    public SqlStatement DependentStatement { get; set; }
}
