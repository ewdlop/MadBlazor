#if false
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlGeneratorApp.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlGeneratorApp.Services
{
    public class SqlGenerator
    {
        private readonly TSqlParser parser;
        private readonly SqlScriptGenerator generator;

        public SqlGenerator(SqlVersion sqlVersion = SqlVersion.Sql160)
        {
            parser = new TSql160Parser(false);
            generator = new Sql160ScriptGenerator(new SqlScriptGeneratorOptions
            {
                KeywordCasing = KeywordCasing.Uppercase,
                IncludeSemicolons = true,
                AlignClauseBodies = true
            });
        }

        public string GenerateStoredProcedure(
            string procedureName,
            List<ParameterDefinition> parameters,
            string procedureBody)
        {
            var createProcedure = new CreateProcedureStatement
            {
                ProcedureReference = new ProcedureReference
                {
                    Name = new SchemaObjectName
                    {
                        SchemaIdentifier = new Identifier("dbo"),
                        BaseIdentifier = new Identifier(procedureName)
                    }
                },
                StatementList = new StatementList()
            };

            // Add parameters
            foreach (var param in parameters)
            {
                createProcedure.Parameters.Add(new ProcedureParameter
                {
                    VariableName = new Identifier($"@{param.Name}"),
                    DataType = new SqlDataTypeReference
                    {
                        Name = new SchemaObjectName
                        {
                            BaseIdentifier = new Identifier(param.DataType)
                        }
                    },
                    IsOutput = param.IsOutput
                });
            }

            // Parse and add procedure body
            IList<ParseError> errors;
            var fragment = parser.Parse(new StringReader(procedureBody), out errors);
            if (errors.Count > 0)
            {
                throw new Exception($"Error parsing procedure body: {string.Join(", ", errors)}");
            }

            var batch = fragment as TSqlBatch;
            if (batch != null)
            {
                createProcedure.StatementList.Statements.AddRange(batch.Statements);
            }

            string sql;
            generator.GenerateScript(createProcedure, out sql);
            return sql;
        }

        public string GenerateCTE(
            string cteName,
            List<string> columnNames,
            string baseQuery,
            bool isRecursive = false)
        {
            var commonTableExpression = new CommonTableExpression
            {
                ExpressionName = new Identifier(cteName),
                Columns = new List<Identifier>()
            };

            // Add column names
            foreach (var column in columnNames)
            {
                commonTableExpression.Columns.Add(new Identifier(column));
            }

            // Parse the base query
            IList<ParseError> errors;
            var fragment = parser.Parse(new StringReader(baseQuery), out errors);
            if (errors.Count > 0)
            {
                throw new Exception($"Error parsing CTE query: {string.Join(", ", errors)}");
            }

            var batch = fragment as TSqlBatch;
            if (batch?.Statements.Count > 0 && batch.Statements[0] is SelectStatement selectStatement)
            {
                commonTableExpression.QueryExpression = selectStatement.QueryExpression;
            }
            else
            {
                throw new Exception("Base query must be a SELECT statement");
            }

            // Create the WITH clause
            var withClause = new WithCtesAndXmlNamespaces
            {
                IsRecursive = isRecursive
            };
            withClause.CommonTableExpressions.Add(commonTableExpression);

            // Create a dummy select to generate the complete CTE
            var dummySelect = new SelectStatement
            {
                WithCtesAndXmlNamespaces = withClause,
                QueryExpression = new QuerySpecification
                {
                    SelectElements = new List<SelectElement>
                {
                    new SelectScalarExpression { Expression = new IntegerLiteral { Value = "1" } }
                }
                }
            };

            string sql;
            generator.GenerateScript(dummySelect, out sql);
            return sql;
        }

        public class Example
        {

            public static void Main()
            {
                var generator = new SqlGenerator();

                // Generate a stored procedure
                var parameters = new List<ParameterDefinition>
        {
            new ParameterDefinition { Name = "CustomerId", DataType = "int" },
            new ParameterDefinition { Name = "OrderDate", DataType = "datetime" },
            new ParameterDefinition { Name = "TotalAmount", DataType = "decimal", IsOutput = true }
        };

                string procBody = @"
            SELECT @TotalAmount = SUM(Amount)
            FROM Orders
            WHERE CustomerId = @CustomerId
            AND OrderDate = @OrderDate";

                string storedProc = generator.GenerateStoredProcedure(
                    "GetCustomerOrderTotal",
                    parameters,
                    procBody
                );

                Console.WriteLine("Generated Stored Procedure:");
                Console.WriteLine(storedProc);

                // Generate a CTE
                var columns = new List<string> { "EmployeeId", "ManagerId", "Level" };
                string baseQuery = @"
            SELECT 
                EmployeeId,
                ManagerId,
                1 as Level
            FROM Employees
            WHERE ManagerId IS NULL
            UNION ALL
            SELECT 
                e.EmployeeId,
                e.ManagerId,
                cte.Level + 1
            FROM Employees e
            INNER JOIN EmployeeHierarchy cte ON e.ManagerId = cte.EmployeeId";

                string cte = generator.GenerateCTE(
                    "EmployeeHierarchy",
                    columns,
                    baseQuery,
                    isRecursive: true
                );

                Console.WriteLine("\nGenerated CTE:");
                Console.WriteLine(cte);
            }
        }
    }
}
#endif