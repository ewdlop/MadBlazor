//using Microsoft.EntityFrameworkCore;
//using Microsoft.SqlServer.TransactSql.ScriptDom;
//using SqlGeneratorApp.Models;
//using SqlGeneratorApp.Visitor;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Linq.Expressions;

//namespace SqlGeneratorApp.Services;

//public partial class CteGenerator
//{
//    private readonly TSql160Parser parser;
//    private readonly Sql170ScriptGenerator generator;

//    public CteGenerator()
//    {
//        parser = new TSql160Parser(false);
//        generator = new Sql170ScriptGenerator(new SqlScriptGeneratorOptions
//        {
//            KeywordCasing = KeywordCasing.Uppercase,
//            IncludeSemicolons = true,
//            AlignClauseBodies = true
//        });
//    }

//    public string GenerateCte<T>(CteDefinition<T> cte)
//    {
//        var withClause = new WithCtesAndXmlNamespaces
//        {
//            //IsRecursive = cte.IsRecursive
//        };

//        var commonTableExpression = new CommonTableExpression
//        {
//            ExpressionName = new Identifier()
//            {
//                Value = cte.Name
//            },
//            Columns = cte.Columns.Select(c => new Identifier(c)).ToList()
//        };

//        // Convert LINQ expression to SQL
//        var baseQuerySql = LinqToSql(cte.BaseQuery);

//        if (cte.IsRecursive && cte.RecursiveQuery != null)
//        {
//            var recursiveQuerySql = LinqToSql(cte.RecursiveQuery);
//            baseQuerySql = $"{baseQuerySql}\nUNION ALL\n{recursiveQuerySql}";
//        }

//        // Parse the generated SQL
//        IList<ParseError> errors;
//        var fragment = parser.Parse(new StringReader(baseQuerySql), out errors);

//        if (errors.Count > 0)
//            throw new Exception($"Error parsing CTE query: {string.Join(", ", errors)}");
       

//        var selectStatement = fragment.BatchesCollection[0].Statements[0] as SelectStatement;
//        commonTableExpression.QueryExpression = selectStatement.QueryExpression;

//        withClause.CommonTableExpressions.Add(commonTableExpression);

//        // Create final SELECT statement using the CTE
//        var finalSelect = new SelectStatement
//        {
//            WithCtesAndXmlNamespaces = withClause,
//            QueryExpression = new QuerySpecification
//            {
//                SelectElements = cte.Columns.Select(c =>
//                    new SelectScalarExpression
//                    {
//                        Expression = new ColumnReferenceExpression
//                        {
//                            MultiPartIdentifier = new MultiPartIdentifier(
//                                new[] { new Identifier(cte.Name), new Identifier(c) }
//                            )
//                        }
//                    }).ToList<SelectElement>()
//            }
//        };

//        string sql;
//        generator.GenerateScript(finalSelect, out sql);
//        return sql;
//    }

//    private string LinqToSql<T>(Expression<Func<IQueryable<T>, IQueryable<T>>> expression)
//    {
//        // This is a simplified LINQ to SQL conversion
//        // In a real implementation, you'd want to use a proper LINQ provider
//        var visitor = new LinqToSqlVisitor();
//        return visitor.TranslateExpression(expression);
//    }

//    // Example usage with Employee hierarchy
//    public static void Main()
//    {
//        var generator = new CteGenerator();

//        var employeeHierarchy = new CteDefinition<Employee>
//        {
//            Name = "EmployeeHierarchy",
//            IsRecursive = true,
//            Columns = new List<string> { "EmployeeId", "ManagerId", "Level", "Path" },
//            BaseQuery = q => q.Where(e => e.ManagerId == null)
//                           .Select(e => new Employee()
//                           {
//                               e.EmployeeId,
//                               e.ManagerId,
//                               Level = 1,
//                               Path = e.EmployeeId.ToString()
//                           }).AsQueryable(),
//            RecursiveQuery = q => q.Join(
//                DbSet<Employee>(),
//                h => h.EmployeeId,
//                e => e.ManagerId,
//                (h, e) => new
//                {
//                    e.EmployeeId,
//                    e.ManagerId,
//                    Level = h.Level + 1,
//                    Path = h.Path + "/" + e.EmployeeId
//                })
//        };

//        string sql = generator.GenerateCte(employeeHierarchy);
//        Console.WriteLine(sql);
//    }
//}
