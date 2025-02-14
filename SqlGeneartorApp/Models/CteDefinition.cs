using System.Linq.Expressions;

namespace SqlGeneratorApp.Models;

public class CteDefinition<T>
{
    public string Name { get; set; }
    public bool IsRecursive { get; set; }
    public List<string> Columns { get; set; } = new List<string>();
    public Expression<Func<IQueryable<T>, IQueryable<T>>> BaseQuery { get; set; }
    public Expression<Func<IQueryable<T>, IQueryable<T>>> RecursiveQuery { get; set; }
}