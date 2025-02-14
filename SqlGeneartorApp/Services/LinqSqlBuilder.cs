namespace SqlGeneratorApp.Services;

public partial class LinqSqlBuilder
{
    // Example usage methods
    public static string CreateEmployeeHierarchyCte()
    {
        var cte = new CteBuilder("EmployeeHierarchy", recursive: true)
            .AddColumns("EmployeeId", "ManagerId", "Level", "Path")
            .AddBaseQuery(@"
                    SELECT 
                        EmployeeId,
                        ManagerId,
                        1 as Level,
                        CAST(EmployeeId as VARCHAR(MAX)) as Path
                    FROM Employees
                    WHERE ManagerId IS NULL")
            .AddBaseQuery(@"
                    SELECT 
                        e.EmployeeId,
                        e.ManagerId,
                        eh.Level + 1,
                        CAST(eh.Path + ',' + CAST(e.EmployeeId as VARCHAR(MAX)) as VARCHAR(MAX))
                    FROM Employees e
                    INNER JOIN EmployeeHierarchy eh ON e.ManagerId = eh.EmployeeId")
            .Build();

        return cte + "\nSELECT * FROM EmployeeHierarchy ORDER BY Path;";
    }

    public static string CreateOrderSummaryProc()
    {
        return new StoredProcedureBuilder("GetOrderSummary")
            .AddParameter("CustomerId", "INT")
            .AddParameter("StartDate", "DATE")
            .AddParameter("EndDate", "DATE")
            .AddParameter("TotalOrders", "INT", isOutput: true)
            .AddParameter("TotalAmount", "DECIMAL(18,2)", isOutput: true)
            .AddStatement(@"
                    SELECT 
                        @TotalOrders = COUNT(*),
                        @TotalAmount = SUM(Amount)
                    FROM Orders
                    WHERE CustomerId = @CustomerId
                    AND OrderDate BETWEEN @StartDate AND @EndDate")
            .AddReturn("@TotalOrders")
            .Build();
    }
}