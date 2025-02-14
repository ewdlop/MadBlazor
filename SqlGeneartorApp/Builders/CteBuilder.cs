using System.Text;

namespace SqlGeneratorApp.Services;

public class CteBuilder
{
    private string cteName;
    private List<string> columns = new List<string>();
    private List<string> baseQueries = new List<string>();
    private bool isRecursive;

    public CteBuilder(string name, bool recursive = false)
    {
        cteName = name;
        isRecursive = recursive;
    }

    public CteBuilder AddColumns(params string[] columnNames)
    {
        columns.AddRange(columnNames);
        return this;
    }

    public CteBuilder AddBaseQuery(string query)
    {
        baseQueries.Add(query);
        return this;
    }

    public string Build()
    {
        var sql = new StringBuilder();
        sql.Append("WITH ");
        if (isRecursive) sql.Append("RECURSIVE ");
        sql.Append(cteName);

        if (columns.Any())
        {
            sql.Append(" (")
               .Append(string.Join(", ", columns))
               .Append(")");
        }

        sql.AppendLine(" AS (");

        // Combine base queries with UNION ALL if recursive
        sql.AppendLine(baseQueries
            .Select(q => "    " + q)
            .Aggregate((a, b) => a + "\n    UNION ALL\n" + b));

        sql.AppendLine(")");
        return sql.ToString();
    }
}