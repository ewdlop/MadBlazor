using Microsoft.SemanticKernel;

namespace SqlGeneratorApp.Services;

public class SqlGeneratorService
{
    public void Generate()
    {
        // Setup
        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Trace));

        // Generate CTE
        //var cteResult = await kernel.InvokeAsync("SqlGenerator", "GenerateCte", new KernelArguments
        //{
        //    { "cteName", "EmployeeHierarchy" },
        //    { "columns", "EmployeeId, ManagerId, Level" },
        //    { "baseQuery", "SELECT EmployeeId, ManagerId, 1 as Level FROM Employees WHERE ManagerId IS NULL" },
        //    { "isRecursive", true }
        //});
    }
}
