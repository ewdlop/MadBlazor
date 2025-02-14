using Microsoft.EntityFrameworkCore;
using SqlGeneratorApp.Models;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SqlGeneratorApp.Contexts;

// DbContext
public class OrderManagementContext : DbContext
{
    public OrderManagementContext(DbContextOptions<OrderManagementContext> options)
        : base(options)
    {
    }


    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).UseIdentityColumn();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Customers");
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId).UseIdentityColumn();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
        });

        // OrderDetail configuration
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId);
            entity.Property(e => e.OrderDetailId).UseIdentityColumn();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(d => d.Order)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetails_Products");
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).UseIdentityColumn();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
    }
}
