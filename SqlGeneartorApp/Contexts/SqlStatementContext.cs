using Microsoft.EntityFrameworkCore;
using SqlGeneratorApp.Models;
using System.Threading;

namespace SqlGeneratorApp.Contexts;

public class SqlStatementContext : DbContext
{
    public SqlStatementContext(DbContextOptions<SqlStatementContext> options)
        : base(options)
    {
    }

    public DbSet<DatabaseObject> DatabaseObjects { get; set; }
    public DbSet<SqlStatement> SqlStatements { get; set; }
    public DbSet<SqlStatementDependency> SqlStatementDependencies { get; set; }
    public DbSet<SqlStatementVersion> SqlStatementVersions { get; set; }
    public DbSet<SqlStatementParameter> SqlStatementParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DatabaseObject configuration
        modelBuilder.Entity<DatabaseObject>(entity =>
        {
            entity.HasIndex(e => new { e.SchemaName, e.ObjectName }).IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // SqlStatement configuration
        modelBuilder.Entity<SqlStatement>(entity =>
        {
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(d => d.DatabaseObject)
                .WithMany(p => p.SqlStatements)
                .HasForeignKey(d => d.DatabaseObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure many-to-many self-referencing relationship for dependencies
            entity.HasMany(s => s.Dependencies)
                .WithOne(d => d.ParentStatement)
                .HasForeignKey(d => d.ParentStatementId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(s => s.DependentStatements)
                .WithOne(d => d.DependentStatement)
                .HasForeignKey(d => d.DependentStatementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SqlStatementVersion configuration
        modelBuilder.Entity<SqlStatementVersion>(entity =>
        {
            entity.Property(e => e.ChangedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(d => d.SqlStatement)
                .WithMany()
                .HasForeignKey(d => d.SqlStatementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SqlStatementParameter configuration
        modelBuilder.Entity<SqlStatementParameter>(entity =>
        {
            entity.HasOne(d => d.SqlStatement)
                .WithMany()
                .HasForeignKey(d => d.SqlStatementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // Example usage methods
    public async Task<SqlStatement> AddSqlStatementAsync(SqlStatement statement, CancellationToken cancellationToken = default)
    {
        using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        try
        {
            statement.CreatedDate = DateTime.UtcNow;
            SqlStatements.Add(statement);
            await SaveChangesAsync(cancellationToken);

            // Create initial version
            var version = new SqlStatementVersion
            {
                SqlStatementId = statement.SqlStatementId,
                SqlText = statement.SqlText,
                VersionNumber = 1,
                ChangedBy = statement.CreatedBy,
                ChangeDescription = "Initial version"
            };
            SqlStatementVersions.Add(version);
            await SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return statement;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SqlStatement> UpdateSqlStatementAsync(SqlStatement statement, CancellationToken cancellationToken = default)
    {
        using var transaction = await Database.BeginTransactionAsync();
        try
        {
            var existingStatement = await SqlStatements
                .FindAsync([statement.SqlStatementId], cancellationToken: cancellationToken);

            if (existingStatement == null)
                throw new KeyNotFoundException($"SQL Statement with ID {statement.SqlStatementId} not found.");

            // Create new version
            var lastVersion = await SqlStatementVersions
                .Where(v => v.SqlStatementId == statement.SqlStatementId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstAsync(cancellationToken);

            var newVersion = new SqlStatementVersion
            {
                SqlStatementId = statement.SqlStatementId,
                SqlText = statement.SqlText,
                VersionNumber = lastVersion.VersionNumber + 1,
                ChangedBy = statement.ModifiedBy,
                ChangeDescription = "Updated SQL statement"
            };

            // Update statement
            existingStatement.SqlText = statement.SqlText;
            existingStatement.ModifiedBy = statement.ModifiedBy;
            existingStatement.ModifiedDate = DateTime.UtcNow;

            SqlStatementVersions.Add(newVersion);
            await SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return existingStatement;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
