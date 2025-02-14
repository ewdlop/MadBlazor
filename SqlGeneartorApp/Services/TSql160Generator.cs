namespace SqlGeneratorApp.Services
{
    using Microsoft.SqlServer.TransactSql.ScriptDom;
    using System.Text;

    public class TSql160Generator
    {
        private readonly TSql160Parser _parser;
        private readonly SqlScriptGenerator _generator;

        public TSql160Generator()
        {
            _parser = new TSql160Parser(false);
            _generator = new Sql160ScriptGenerator(
                new SqlScriptGeneratorOptions
                {
                    //IncludeHeaders = true,
                    KeywordCasing = KeywordCasing.Uppercase
                });
        }

        public string GenerateCreateTable()
        {
            var createTableStatement = new CreateTableStatement
            {
                SchemaObjectName = new SchemaObjectName
                {
                    Identifiers = {
                    new Identifier { Value = "dbo" },
                    new Identifier { Value = "Orders" }
                }
                },
                Definition = new TableDefinition
                {
                    ColumnDefinitions =
                {
                    new ColumnDefinition
                    {
                        ColumnIdentifier = new Identifier { Value = "OrderId" },
                        DataType = new SqlDataTypeReference { SqlDataTypeOption = SqlDataTypeOption.Int },
                        Constraints = { }//new ConstraintDefinition() }
                    },
                    new ColumnDefinition
                    {
                        ColumnIdentifier = new Identifier { Value = "CustomerId" },
                        DataType = new SqlDataTypeReference { SqlDataTypeOption = SqlDataTypeOption.Int }
                    },
                    new ColumnDefinition
                    {
                        ColumnIdentifier = new Identifier { Value = "OrderDate" },
                        DataType = new SqlDataTypeReference { SqlDataTypeOption = SqlDataTypeOption.DateTime }
                    },
                    new ColumnDefinition
                    {
                        ColumnIdentifier = new Identifier { Value = "TotalAmount" },
                        DataType = new SqlDataTypeReference
                        {
                            SqlDataTypeOption = SqlDataTypeOption.Decimal,
                            Parameters = {
                                new IntegerLiteral { Value = "18" },
                                new IntegerLiteral { Value = "2" }
                            }
                        }
                    }
                }
                }
            };

            string sql = GenerateScript(createTableStatement);
            return sql;
        }

        public string GenerateSelect()
        {
            var selectStatement = new SelectStatement
            {
                QueryExpression = new QuerySpecification
                {
                    SelectElements =
                {
                    new SelectScalarExpression
                    {
                        Expression = new ColumnReferenceExpression
                        {
                            MultiPartIdentifier = new MultiPartIdentifier
                            {
                                Identifiers = { new Identifier { Value = "OrderId" } }
                            }
                        }
                    },
                    new SelectScalarExpression
                    {
                        Expression = new ColumnReferenceExpression
                        {
                            MultiPartIdentifier = new MultiPartIdentifier
                            {
                                Identifiers = { new Identifier { Value = "CustomerId" } }
                            }
                        }
                    }
                },
                    FromClause = new FromClause
                    {
                        TableReferences =
                    {
                        new NamedTableReference
                        {
                            SchemaObject = new SchemaObjectName
                            {
                                Identifiers =
                                {
                                    new Identifier { Value = "dbo" },
                                    new Identifier { Value = "Orders" }
                                }
                            }
                        }
                    }
                    },
                    WhereClause = new WhereClause
                    {
                        SearchCondition = new BooleanComparisonExpression
                        {
                            FirstExpression = new ColumnReferenceExpression
                            {
                                MultiPartIdentifier = new MultiPartIdentifier
                                {
                                    Identifiers = { new Identifier { Value = "TotalAmount" } }
                                }
                            },
                            ComparisonType = BooleanComparisonType.GreaterThan,
                            SecondExpression = new NumericLiteral { Value = "1000" }
                        }
                    }
                }
            };

            string sql = GenerateScript(selectStatement);
            return sql;
        }

        public string GenerateInsert()
        {
            var insertStatement = new InsertStatement
            {
                InsertSpecification = new InsertSpecification
                {
                    Target = new NamedTableReference
                    {
                        SchemaObject = new SchemaObjectName
                        {
                            Identifiers =
                        {
                            new Identifier { Value = "dbo" },
                            new Identifier { Value = "Orders" }
                        }
                        }
                    },
                    Columns =
                {
                    new ColumnReferenceExpression
                    {
                        MultiPartIdentifier = new MultiPartIdentifier
                        {
                            Identifiers = { new Identifier { Value = "CustomerId" } }
                        }
                    },
                    new ColumnReferenceExpression
                    {
                        MultiPartIdentifier = new MultiPartIdentifier
                        {
                            Identifiers = { new Identifier { Value = "OrderDate" } }
                        }
                    },
                    new ColumnReferenceExpression
                    {
                        MultiPartIdentifier = new MultiPartIdentifier
                        {
                            Identifiers = { new Identifier { Value = "TotalAmount" } }
                        }
                    }
                },
                    InsertSource = new ValuesInsertSource
                    {
                        RowValues =
                    {
                        new RowValue
                        {
                            ColumnValues =
                            {
                                new IntegerLiteral { Value = "1" },
                                new FunctionCall
                                {
                                    FunctionName = new Identifier { Value = "GETDATE" }
                                },
                                new NumericLiteral { Value = "1500.50" }
                            }
                        }
                    }
                    }
                }
            };

            string sql = GenerateScript(insertStatement);
            return sql;
        }

        private string GenerateScript(TSqlFragment fragment)
        {
            var builder = new StringBuilder();
            _generator.GenerateScript(fragment, out string sql);
            return sql;
        }
    }
}
