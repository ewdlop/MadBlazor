using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SqlGeneratorApp.Models;

[Table("SqlStatementParameters")]
public class SqlStatementParameter
{
    [Key]
    public int ParameterId { get; set; }

    public int SqlStatementId { get; set; }

    [Required]
    [MaxLength(128)]
    public string ParameterName { get; set; }

    [Required]
    [MaxLength(50)]
    public string DataType { get; set; }

    public bool IsRequired { get; set; }

    [MaxLength(500)]
    public string DefaultValue { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    // Navigation property
    public SqlStatement SqlStatement { get; set; }
}