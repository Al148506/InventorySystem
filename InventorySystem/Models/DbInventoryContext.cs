﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Models;

public partial class DbInventoryContext : DbContext
{
    public DbInventoryContext()
    {
    }

    public DbInventoryContext(DbContextOptions<DbInventoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<History> Histories { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRol> UserRols { get; set; }

    

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser);

            entity.ToTable("User");

            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.Mail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Users)
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
