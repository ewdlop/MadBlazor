//using System.Linq.Expressions;
//using System.Text;

//namespace SqlGeneratorApp.Visitor;

//// Helper class for LINQ to SQL conversion
//public class LinqToSqlVisitor : SqlExpressionVisitor 
//{
//    private StringBuilder sql = new StringBuilder();

//    public string TranslateExpression<T>(Expression<Func<IQueryable<T>, IQueryable<T>>> expression)
//    {
//        Visit(expression);
//        return sql.ToString();
//    }

//    protected override Expression VisitMethodCall(MethodCallExpression node)
//    {
//        if (node.Method.DeclaringType == typeof(Queryable))
//        {
//            switch (node.Method.Name)
//            {
//                case "Where":
//                    sql.Append("SELECT * FROM ");
//                    Visit(node.Arguments[0]);
//                    sql.Append(" WHERE ");
//                    VisitLambda(node.Arguments[1] as LambdaExpression);
//                    break;

//                case "Select":
//                    sql.Append("SELECT ");
//                    VisitLambda(node.Arguments[1] as LambdaExpression);
//                    sql.Append(" FROM ");
//                    Visit(node.Arguments[0]);
//                    break;

//                case "Join":
//                    HandleJoin(node);
//                    break;
//            }
//        }
//        return node;
//    }

//    private void HandleJoin(MethodCallExpression node)
//    {
//        sql.Append("SELECT * FROM ");
//        Visit(node.Arguments[0]);
//        sql.Append(" INNER JOIN ");
//        Visit(node.Arguments[1]);
//        sql.Append(" ON ");
//        VisitLambda(node.Arguments[2] as LambdaExpression);
//        sql.Append(" = ");
//        VisitLambda(node.Arguments[3] as LambdaExpression);
//    }

//    protected override Expression VisitBinary(Microsoft.SqlServer.TransactSql.ScriptDom.BinaryExpression node)
//    {
//        Visit(node.Left);

//        switch (node.NodeType)
//        {
//            case ExpressionType.Equal:
//                sql.Append(" = ");
//                break;
//            case ExpressionType.NotEqual:
//                sql.Append(" <> ");
//                break;
//            case ExpressionType.AndAlso:
//                sql.Append(" AND ");
//                break;
//            case ExpressionType.OrElse:
//                sql.Append(" OR ");
//                break;
//                // Add other operators as needed
//        }

//        Visit(node.Right);
//        return node;
//    }

//    protected override Expression VisitMember(MemberExpression node)
//    {
//        sql.Append(node.Member.Name);
//        return node;
//    }

//    protected override Expression VisitConstant(ConstantExpression node)
//    {
//        if (node.Value == null)
//        {
//            sql.Append("NULL");
//        }
//        else if (node.Value is string)
//        {
//            sql.Append($"'{node.Value}'");
//        }
//        else
//        {
//            sql.Append(node.Value.ToString());
//        }
//        return node;
//    }
//}
