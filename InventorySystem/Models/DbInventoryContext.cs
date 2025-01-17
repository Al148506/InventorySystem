using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InventorySystem.Models;

public partial class DbInventoryContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public DbInventoryContext(DbContextOptions<DbInventoryContext> options, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
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

    private void AddAuditEntries()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
        // Lista temporal para las entradas de auditoría
        var auditEntries = new List<ChangeLog>();

        foreach (var entry in entries)
        {
            var auditEntry = new ChangeLog
            {
                TableName = entry.Entity.GetType().Name,
                DateMod = DateTime.Now,
                UserId = GetCurrentUserId(), // Implementar un método para obtener el usuario actual
                PrimaryKey = GetPrimaryKeyValue(entry) // Obtener el valor de la clave primaria
            };

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.TypeAction = "Creación";
                    auditEntry.NewValues = SerializeProperties(GetAddedProperties(entry));
                    break;

                case EntityState.Modified:
                    auditEntry.TypeAction = "Actualización";
                    auditEntry.OldValues = SerializeProperties(GetOriginalValues(entry));
                    auditEntry.NewValues = SerializeProperties(GetCurrentValues(entry));
                    auditEntry.AffectedColumns = GetModifiedColumns(entry);
                    break;

                case EntityState.Deleted:
                    auditEntry.TypeAction = "Eliminación";
                    auditEntry.OldValues = SerializeProperties(GetDeletedProperties(entry));
                    break;
            }

            // Agregar el registro a la tabla de auditoría
            auditEntries.Add(auditEntry);
        }
        // Agregar las entradas de auditoría al contexto después de iterar
        ChangeLogs.AddRange(auditEntries);
    }

    // Obtener Propiedades Agregadas
    private Dictionary<string, object> GetAddedProperties(EntityEntry entry)
    {
        return entry.Properties
            .Where(p => p.CurrentValue != null)
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
    }

    // Obtener Valores Originales
    private Dictionary<string, object> GetOriginalValues(EntityEntry entry)
    {
        return entry.Properties
             .Where(p => p.OriginalValue != null)
            .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
    }

    // Obtener Valores Actuales
    private Dictionary<string, object> GetCurrentValues(EntityEntry entry)
    {
        return entry.Properties
            .Where(p => p.IsModified)
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
    }

    // Obtener Propiedades Eliminadas
    private Dictionary<string, object> GetDeletedProperties(EntityEntry entry)
    {
        return entry.Properties
            .Where(p => p.OriginalValue != null)
            .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
    }

    // Obtener Columnas Modificadas
    private string GetModifiedColumns(EntityEntry entry)
    {
        var modifiedColumns = entry.Properties
            .Where(p => p.IsModified)
            .Select(p => p.Metadata.Name);

        return string.Join(", ", modifiedColumns);
    }

    // Obtener Valor de la Clave Primaria
    private string GetPrimaryKeyValue(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();

        if (primaryKey == null)
            return "Sin clave primaria";

        var keyValues = primaryKey.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString());

        return string.Join(", ", keyValues);
    }

    // Serializar Propiedades
    private string SerializeProperties(object properties)
    {
        var json= JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = true });
        const int maxLength = 5000;
        if (json.Length > maxLength)
        {
            json = json.Substring(0, maxLength - 3) + "...";
        }
        return json;
    }

    // Obtener el Usuario Actual
    private string GetCurrentUserId()
    {
        // Intenta obtener el valor del correo electrónico del usuario desde la sesión
        var userMail = _httpContextAccessor.HttpContext?.Session.GetString("UserMail");

        // Si no se encuentra el valor, devuelve un mensaje predeterminado
        return !string.IsNullOrEmpty(userMail) ? userMail : "Usuario desconocido";
    }

    public override int SaveChanges()
    {
        AddAuditEntries();
        return base.SaveChanges();
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditEntries();
        return await base.SaveChangesAsync(cancellationToken);
    }




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
                  .HasMaxLength(5000);

            entity.Property(e => e.NewValues)
                  .HasMaxLength(5000);

            entity.Property(e => e.AffectedColumns)
                  .HasMaxLength(255);

            entity.Property(e => e.PrimaryKey)
                  .HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
