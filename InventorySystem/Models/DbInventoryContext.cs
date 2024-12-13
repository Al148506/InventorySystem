﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Models;

public partial class DbInventoryContext : DbContext
{
    private readonly IConfiguration _configuration;
    public DbInventoryContext(DbContextOptions<DbInventoryContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbInventoryContext(DbContextOptions<DbInventoryContext> options)
        : base(options)
    {
    }
    public DbSet<ChangeLog> ChangeLogs { get; set; }
    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<History> Histories { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<UserLogin> UserLogins { get; set; }

    public virtual DbSet<UserRol> UserRols { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("DbContext");
        optionsBuilder.UseSqlServer(connectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory);

            entity.ToTable("Category");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<History>(entity =>
        {
            entity.HasKey(e => e.IdHistory);

            entity.ToTable("History");

            entity.Property(e => e.ChangeType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CurrentValue).HasColumnType("text");
            entity.Property(e => e.DateMod).HasColumnType("datetime");
            entity.Property(e => e.PreviousValue).HasColumnType("text");

            entity.HasOne(d => d.IdProdNavigation).WithMany(p => p.Histories)
                .HasForeignKey(d => d.IdProd)
                .HasConstraintName("FK_History_Product");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Histories)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_History_User");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.IdLocation);

            entity.ToTable("Location");

            entity.Property(e => e.LocationName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProd);

            entity.ToTable("Product");

            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ImageRoot)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.LastModDate).HasColumnType("datetime");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdCategory)
                .HasConstraintName("FK_Product_Category");

            entity.HasOne(d => d.Location).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_Product_Location");
        });

        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK_User");

            entity.ToTable("UserLogin");

            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.LastModDate).HasColumnType("datetime");
            entity.Property(e => e.UserMail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserPassword)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.UserLogins)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_User_UserRol");
        });

        modelBuilder.Entity<UserRol>(entity =>
        {
            entity.HasKey(e => e.IdRol);

            entity.ToTable("UserRol");

            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.RolName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ChangeLog>(entity =>
        {
            // Define el nombre de la tabla
            entity.ToTable("ChangeLog");

            // Configuración de la clave primaria
            entity.HasKey(e => e.Id);

            // Configuración de las propiedades
            entity.Property(e => e.Id)
                  .IsRequired()
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.TypeAction)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.TableName)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.Property(e => e.DateMod)
                  .IsRequired()
                  .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.OldValues)
                  .HasMaxLength(255);

            entity.Property(e => e.NewValues)
                  .HasMaxLength(255);

            entity.Property(e => e.AffectedColumns)
                  .HasMaxLength(255);

            entity.Property(e => e.PrimaryKey)
                  .HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
