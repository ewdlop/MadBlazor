using System.Text;

namespace SqlGeneratorApp.Services;

public partial class LinqSqlBuilder
{
    public class StoredProcedureBuilder
    {
        private string procedureName;
        private List<Parameter> parameters = new List<Parameter>();
        private List<string> statements = new List<string>();
        private List<string> returns = new List<string>();

        public class Parameter
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsOutput { get; set; }
            public string DefaultValue { get; set; }
        }

        public StoredProcedureBuilder(string name)
        {
            procedureName = name;
        }

        public StoredProcedureBuilder AddParameter(string name, string type, bool isOutput = false, string defaultValue = null)
        {
            parameters.Add(new Parameter
            {
                Name = name,
                Type = type,
                IsOutput = isOutput,
                DefaultValue = defaultValue
            });
            return this;
        }

        public StoredProcedureBuilder AddStatement(string sql)
        {
            statements.Add(sql);
            return this;
        }

        public StoredProcedureBuilder AddReturn(string returnValue)
        {
            returns.Add(returnValue);
            return this;
        }

        public string Build()
        {
            var sql = new StringBuilder();
            sql.AppendLine($"CREATE OR ALTER PROCEDURE {procedureName}");

            // Parameters
            if (parameters.Any())
            {
                sql.AppendLine(parameters
                    .Select(p =>
                    {
                        var param = $"    @{p.Name} {p.Type}";
                        if (p.IsOutput) param += " OUTPUT";
                        if (p.DefaultValue != null) param += $" = {p.DefaultValue}";
                        return param;
                    })
                    .Aggregate((a, b) => a + ",\n" + b));
            }

            sql.AppendLine("AS");
            sql.AppendLine("BEGIN");
            sql.AppendLine("    SET NOCOUNT ON;");

            // Statements
            foreach (var statement in statements)
            {
                sql.AppendLine($"    {statement}");
            }

            // Returns
            if (returns.Any())
            {
                sql.AppendLine("    RETURN " + string.Join(" + ", returns));
            }

            sql.AppendLine("END");
            return sql.ToString();
        }
    }
}