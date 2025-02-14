#if false
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlGeneratorApp.Services;

public class TSqlGenerator
{
    private readonly TSql160Parser parser;
    private readonly Sql160ScriptGenerator generator;

    public TSqlGenerator()
    {
        parser = new TSql160Parser(false);
        generator = new Sql160ScriptGenerator(new SqlScriptGeneratorOptions
        {
            KeywordCasing = KeywordCasing.Uppercase,
            IncludeSemicolons = true,
            AlignClauseBodies = true
        });
    }

    public class TableDefinition
    {
        public string SchemaName { get; set; } = "dbo";
        public string TableName { get; set; }
        public List<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
        public List<TableConstraint> Constraints { get; set; } = new List<TableConstraint>();
    }

    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; } = true;
        public string DefaultValue { get; set; }
        public bool IsIdentity { get; set; }
        public string ComputedColumnExpression { get; set; }
    }

    public class TableConstraint
    {
        public string Name { get; set; }
        public ConstraintType Type { get; set; }
        public List<string> Columns { get; set; } = new List<string>();
        public string Expression { get; set; }
        public string ReferencedTable { get; set; }
        public List<string> ReferencedColumns { get; set; } = new List<string>();
    }

    public enum ConstraintType
    {
        PrimaryKey,
        ForeignKey,
        Unique,
        Check
    }

    public string GenerateTable(TableDefinition table)
    {
        var createTable = new CreateTableStatement
        {
            SchemaObjectName = new SchemaObjectName
            {
                SchemaIdentifier = new Identifier()
                {
                    Value = table.SchemaName
                },
                BaseIdentifier = new Identifier(table.TableName)
            }
        };

        // Add columns
        foreach (var col in table.Columns)
        {
            var column = new ColumnDefinition
            {
                ColumnIdentifier = new Identifier(col.Name),
                DataType = new SqlDataTypeReference
                {
                    Name = new SchemaObjectName { BaseIdentifier = new Identifier(col.DataType) }
                },
                Nullable = col.IsNullable
            };

            if (col.IsIdentity)
            {
                column.IdentityOptions = new IdentityOptions
                {
                    IdentitySeed = new IntegerLiteral { Value = "1" },
                    IdentityIncrement = new IntegerLiteral { Value = "1" }
                };
            }

            if (!string.IsNullOrEmpty(col.DefaultValue))
            {
                column.DefaultConstraint = new DefaultConstraint
                {
                    Expression = ParseExpression(col.DefaultValue)
                };
            }

            if (!string.IsNullOrEmpty(col.ComputedColumnExpression))
            {
                column.ComputedColumnExpression = ParseExpression(col.ComputedColumnExpression);
            }

            createTable.Definition.ColumnDefinitions.Add(column);
        }

        // Add constraints
        foreach (var constraint in table.Constraints)
        {
            ConstraintDefinition constraintDef = null;

            switch (constraint.Type)
            {
                case ConstraintType.PrimaryKey:
                    constraintDef = new UniqueConstraintDefinition
                    {
                        IsPrimaryKey = true,
                        Columns = new List<ColumnReferenceExpression>(
                            constraint.Columns.Select(c => new ColumnReferenceExpression
                            {
                                ColumnType = ColumnType.Regular,
                                MultiPartIdentifier = new MultiPartIdentifier(new[] { new Identifier(c) })
                            }))
                    };
                    break;

                case ConstraintType.ForeignKey:
                    constraintDef = new ForeignKeyConstraintDefinition
                    {
                        Columns = new List<ColumnReferenceExpression>(
                            constraint.Columns.Select(c => new ColumnReferenceExpression
                            {
                                ColumnType = ColumnType.Regular,
                                MultiPartIdentifier = new MultiPartIdentifier(new[] { new Identifier(c) })
                            })),
                        ReferenceTableName = new SchemaObjectName
                        {
                            SchemaIdentifier = new Identifier("dbo"),
                            BaseIdentifier = new Identifier(constraint.ReferencedTable)
                        },
                        ReferencedTableColumns = new List<ColumnReferenceExpression>(
                            constraint.ReferencedColumns.Select(c => new ColumnReferenceExpression
                            {
                                ColumnType = ColumnType.Regular,
                                MultiPartIdentifier = new MultiPartIdentifier(new[] { new Identifier(c) })
                            }))
                    };
                    break;

                case ConstraintType.Unique:
                    constraintDef = new UniqueConstraintDefinition
                    {
                        IsPrimaryKey = false,
                        Columns = new List<ColumnReferenceExpression>(
                            constraint.Columns.Select(c => new ColumnReferenceExpression
                            {
                                ColumnType = ColumnType.Regular,
                                MultiPartIdentifier = new MultiPartIdentifier(new[] { new Identifier(c) })
                            }))
                    };
                    break;

                case ConstraintType.Check:
                    constraintDef = new CheckConstraintDefinition
                    {
                        CheckCondition = ParseExpression(constraint.Expression)
                    };
                    break;
            }

            if (constraintDef != null)
            {
                constraintDef.ConstraintIdentifier = new Identifier(constraint.Name);
                createTable.Definition.TableConstraints.Add(constraintDef);
            }
        }

        string sql;
        generator.GenerateScript(createTable, out sql);
        return sql;
    }

    private BooleanExpression ParseExpression(string expression)
    {
        IList<ParseError> errors;
        var fragment = parser.Parse(new StringReader($"SELECT * FROM t WHERE {expression}"), out errors);

        if (errors.Count > 0)
            throw new Exception($"Error parsing expression: {string.Join(", ", errors)}");

        var select = fragment.BatchesCollection[0].Statements[0] as SelectStatement;
        return (select?.QueryExpression as QuerySpecification)?.WhereClause?.SearchCondition;
    }

    // Example usage
    public static void Main()
    {
        var generator = new TSqlGenerator();

        var orderTable = new TableDefinition
        {
            TableName = "Orders",
            Columns = new List<ColumnDefinition>
            {
                new ColumnDefinition
                {
                    Name = "OrderId",
                    DataType = "int",
                    IsNullable = false,
                    IsIdentity = true
                },
                new ColumnDefinition
                {
                    Name = "CustomerId",
                    DataType = "int",
                    IsNullable = false
                },
                new ColumnDefinition
                {
                    Name = "OrderDate",
                    DataType = "datetime2",
                    IsNullable = false,
                    DefaultValue = "GETDATE()"
                },
                new ColumnDefinition
                {
                    Name = "TotalAmount",
                    DataType = "decimal(18,2)",
                    IsNullable = false,
                    DefaultValue = "0"
                },
                new ColumnDefinition
                {
                    Name = "IsDiscounted",
                    DataType = "bit",
                    ComputedColumnExpression = "TotalAmount < 100"
                }
            },
            Constraints = new List<TableConstraint>
            {
                new TableConstraint
                {
                    Name = "PK_Orders",
                    Type = ConstraintType.PrimaryKey,
                    Columns = new List<string> { "OrderId" }
                },
                new TableConstraint
                {
                    Name = "FK_Orders_Customers",
                    Type = ConstraintType.ForeignKey,
                    Columns = new List<string> { "CustomerId" },
                    ReferencedTable = "Customers",
                    ReferencedColumns = new List<string> { "CustomerId" }
                },
                new TableConstraint
                {
                    Name = "CHK_Orders_TotalAmount",
                    Type = ConstraintType.Check,
                    Expression = "TotalAmount >= 0"
                }
            }
        };

        string sql = generator.GenerateTable(orderTable);
        Console.WriteLine(sql);
    }
}
#endif