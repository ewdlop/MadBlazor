using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SqlGeneratorApp.Models;

[Table("DatabaseObjects")]
public class DatabaseObject
{
    [Key]
    public int DatabaseObjectId { get; set; }

    [Required]
    [MaxLength(128)]
    public string SchemaName { get; set; } = "dbo";

    [Required]
    [MaxLength(128)]
    public string ObjectName { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    // Navigation property
    public ICollection<SqlStatement> SqlStatements { get; set; }
}
