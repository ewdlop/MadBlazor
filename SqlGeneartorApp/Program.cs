using Microsoft.EntityFrameworkCore;
using SqlGeneratorApp.Components;
using SqlGeneratorApp.Contexts;
using SqlGeneratorApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<OrderManagementContext>(options =>
{
    options.UseSqlServer("OrderManagementContext").UseAsyncSeeding(async (context, b, cancellationToken) =>
    {

        // Query example
        //var highValueOrders = await context.Set<Order>()
        //    .Include(o => o.Customer)
        //    .Include(o => o.OrderDetails)
        //        .ThenInclude(od => od.Product)
        //    .Where(o => o.TotalAmount > 1000)
        //    .ToListAsync(cancellationToken);

        // Insert example
        var newOrder = new Order
        {
            CustomerId = 1,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 1500.50m
        };

        context.Set<Order>().Add(newOrder);
        await context.SaveChangesAsync(cancellationToken);
    });
});

builder.Services.AddDbContextFactory<SqlStatementContext>(options =>
{
    options.UseSqlServer("SqlStatementContext").UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        var databaseObject = new DatabaseObject
        {
            SchemaName = "dbo",
            ObjectName = "Customers",
            Description = "Customers table"
        };

        var selectStatement = new SqlStatement
        {
            DatabaseObject = databaseObject,
            StatementType = SqlStatementType.Select,
            SqlText = "SELECT * FROM dbo.Customers WHERE CustomerID = @CustomerID",
            Description = "Get customer by ID",
            CreatedBy = "system",
            Parameters = "@CustomerID int"
        };

        context.Set<DatabaseObject>().Add(databaseObject);
        context.Set<SqlStatement>().Add(selectStatement);
        await context.SaveChangesAsync(cancellationToken);
    }
        // Query example
        //var activeSelects = await context.Set<SqlStatement>()
        //    .Include(s => s.DatabaseObject)
        //    .Where(s => s.StatementType == SqlStatementType.Select && s.IsActive)
        //    .ToListAsync(cancellationToken);
        //}
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
