//using Microsoft.SemanticKernel;
//using Microsoft.SqlServer.TransactSql.ScriptDom;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Text;
//using System.Threading.Tasks;
//using System.Linq;
//using SqlGeneratorApp.Plugins;

//namespace SqlGeneratorApp.Plugins;

//public class SqlGeneratorPlugin
//{
//    private readonly TSql160Parser parser;
//    private readonly Sql160ScriptGenerator generator;

//    public SqlGeneratorPlugin()
//    {
//        parser = new TSql160Parser(false);
//        generator = new Sql160ScriptGenerator(new SqlScriptGeneratorOptions
//        {
//            KeywordCasing = KeywordCasing.Uppercase,
//            IncludeSemicolons = true,
//            AlignClauseBodies = true
//        });
//    }

//    [KernelFunction, Description("Generates a CTE (Common Table Expression) based on input parameters")]
//    public async Task<string> GenerateCteAsync(
//        [Description("Name of the CTE")] string cteName,
//        [Description("Comma-separated list of column names")] string columns,
//        [Description("Base query for the CTE")] string baseQuery,
//        [Description("Recursive part of the query (optional)")] string recursiveQuery = "",
//        [Description("Whether the CTE is recursive")] bool isRecursive = false)
//    {
//        try
//        {
//            var commonTableExpression = new CommonTableExpression
//            {
//                ExpressionName = new Identifier()
//                {
//                    Value = cteName
//                },
//                Columns = new List<Identifier>()
//            };

//            // Add column names
//            foreach (var column in columns.Split(',', StringSplitOptions.TrimEntries))
//            {
//                commonTableExpression.Columns.Add(new Identifier()
//                {
//                    Value = column
//                });
//            }

//            // Parse base query
//            var baseQuerySql = ParseQuery(baseQuery);

//            if (isRecursive && !string.IsNullOrEmpty(recursiveQuery))
//            {
//                var recursiveQuerySql = ParseQuery(recursiveQuery);
//                baseQuerySql = CombineQueries(baseQuerySql, recursiveQuerySql);
//            }

//            commonTableExpression.QueryExpression = baseQuerySql;

//            // Create WITH clause
//            var withClause = new WithCtesAndXmlNamespaces
//            {
//                //IsRecursive = isRecursive
//            };
//            withClause.CommonTableExpressions.Add(commonTableExpression);

//            // Create final SELECT statement
//            var selectStatement = new SelectStatement
//            {
//                WithCtesAndXmlNamespaces = withClause,
//                QueryExpression = new QuerySpecification
//                {
//                    SelectElements = CreateSelectElements(cteName, columns)
//                }
//            };

//            string sql;
//            generator.GenerateScript(selectStatement, out sql);
//            return sql;
//        }
//        catch (Exception ex)
//        {
//            throw new Exception($"Error generating CTE: {ex.Message}");
//        }
//    }

//    [KernelFunction, Description("Generates a stored procedure based on input parameters")]
//    public async Task<string> GenerateStoredProcedureAsync(
//        [Description("Name of the stored procedure")] string procedureName,
//        [Description("List of parameters in format: name type [output] [default], separated by semicolons")] string parameters,
//        [Description("Body of the stored procedure")] string procedureBody)
//    {
//        try
//        {
//            var createProcedure = new CreateProcedureStatement
//            {
//                ProcedureReference = new ProcedureReference
//                {
//                    Name = new SchemaObjectName
//                    {
//                        SchemaIdentifier = new Identifier()
//                        {
//                           Value = "dbo"
//                        },
//                        BaseIdentifier = new Identifier()
//                        {
//                            Value = procedureName
//                        }
//                    }
//                },
//                StatementList = new StatementList()
//            };

//            // Parse parameters
//            foreach (var param in parameters.Split(';', StringSplitOptions.TrimEntries))
//            {
//                if (string.IsNullOrEmpty(param)) continue;

//                var parts = param.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//                var paramName = parts[0];
//                var paramType = parts[1];
//                var isOutput = parts.Length > 2 && parts[2].Equals("output", StringComparison.OrdinalIgnoreCase);
//                var defaultValue = parts.Length > 3 ? parts[3] : null;

//                createProcedure.Parameters.Add(new ProcedureParameter
//                {
//                    VariableName = new Identifier()
//                    {
//                        Value = $"@{paramName}"
//                    },
//                    DataType = new SqlDataTypeReference
//                    {
//                        Name = new SchemaObjectName
//                        {
//                            BaseIdentifier = new Identifier()
//                            {
//                                Value = paramType
//                            }
//                        }
//                    },
//                    //IsOutput = isOutput,
//                    Value = defaultValue != null ? ParseExpression(defaultValue) : null
//                });
//            }

//            // Parse procedure body
//            IList<ParseError> errors;
//            var fragment = parser.Parse(new StringReader(procedureBody), out errors);

//            if (errors.Count > 0)
//                throw new Exception($"Error parsing procedure body: {string.Join(", ", errors)}");

//            var batch = fragment as TSqlBatch;
//            if (batch != null)
//            {
//                foreach (var statement in batch.Statements)
//                {
//                    createProcedure.StatementList.Statements.Add(statement);
//                }
//            }

//            string sql;
//            generator.GenerateScript(createProcedure, out sql);
//            return sql;
//        }
//        catch (Exception ex)
//        {
//            throw new Exception($"Error generating stored procedure: {ex.Message}");
//        }
//    }

//    private QueryExpression ParseQuery(string query)
//    {
//        IList<ParseError> errors;
//        var fragment = parser.Parse(new StringReader(query), out errors);

//        if (errors.Count > 0)
//            throw new Exception($"Error parsing query: {string.Join(", ", errors)}");

//        var selectStatement = fragment.BatchesCollection[0].Statements[0] as SelectStatement;
//        return selectStatement?.QueryExpression;
//    }

//    private QueryExpression CombineQueries(QueryExpression baseQuery, QueryExpression recursiveQuery)
//    {
//        return new BinaryQueryExpression
//        {
//            FirstQueryExpression = baseQuery,
//            SecondQueryExpression = recursiveQuery,
//            BinaryQueryExpressionType = BinaryQueryExpressionType.UnionAll
//        };
//    }

//    private List<SelectElement> CreateSelectElements(string cteName, string columns)
//    {
//        var elements = new List<SelectElement>();
//        foreach (var column in columns.Split(',', StringSplitOptions.TrimEntries))
//        {
//            elements.Add(new SelectScalarExpression
//            {
//                Expression = new ColumnReferenceExpression
//                {
//                    MultiPartIdentifier = new MultiPartIdentifier()
//                    {
//                        Identifiers = new[] {
//                            new Identifier()
//                            {
//                                Value = cteName
//                            },
//                            new Identifier() {
//                            Value = column
//                            }
//                        }
//                    }
//                }
//            });
//        }
//        return elements;
//    }

//    private ScalarExpression ParseExpression(string expression)
//    {
//        IList<ParseError> errors;
//        var fragment = parser.Parse(new StringReader($"SELECT {expression}"), out errors);

//        if (errors.Count > 0)
//            throw new Exception($"Error parsing expression: {string.Join(", ", errors)}");

//        var select = fragment.BatchesCollection[0].Statements[0] as SelectStatement;
//        var querySpec = select?.QueryExpression as QuerySpecification;
//        return (querySpec?.SelectElements[0] as SelectScalarExpression)?.Expression;
//    }// Example usage with Semantic Kernel

//    public static async Task Main()
//    {
//        var kernel = Kernel.CreateBuilder()
//            .AddPlugin(new SqlGeneratorPlugin())
//            .Build();

//        // Generate a recursive CTE for employee hierarchy
//        var cteResult = await kernel.InvokeAsync("SqlGenerator", "GenerateCte", new KernelArguments
//            {
//                { "cteName", "EmployeeHierarchy" },
//                { "columns", "EmployeeId, ManagerId, Level, Path" },
//                { "baseQuery", @"
//                        SELECT 
//                            EmployeeId,
//                            ManagerId,
//                            1 as Level,
//                            CAST(EmployeeId as VARCHAR(MAX)) as Path
//                        FROM Employees
//                        WHERE ManagerId IS NULL" },
//                { "recursiveQuery", @"
//                        SELECT 
//                            e.EmployeeId,
//                            e.ManagerId,
//                            eh.Level + 1,
//                            CAST(eh.Path + '/' + CAST(e.EmployeeId as VARCHAR(MAX)) as VARCHAR(MAX))
//                        FROM Employees e
//                        INNER JOIN EmployeeHierarchy eh ON e.ManagerId = eh.EmployeeId" },
//                { "isRecursive", true }
//            });

//        Console.WriteLine("Generated CTE:");
//        Console.WriteLine(cteResult);

//        // Generate a stored procedure
//        var procResult = await kernel.InvokeAsync("SqlGenerator", "GenerateStoredProcedure", new KernelArguments
//            {
//                { "procedureName", "GetOrderSummary" },
//                { "parameters", @"CustomerId int; StartDate datetime; EndDate datetime; TotalOrders int output" },
//                { "procedureBody", @"
//                        SELECT 
//                            @TotalOrders = COUNT(*)
//                        FROM Orders
//                        WHERE CustomerId = @CustomerId
//                        AND OrderDate BETWEEN @StartDate AND @EndDate" }
//            });

//        Console.WriteLine("\nGenerated Stored Procedure:");
//        Console.WriteLine(procResult);
//    }
//}

