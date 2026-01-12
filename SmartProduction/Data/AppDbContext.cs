using Microsoft.EntityFrameworkCore;
using SmartProduction.Models;

namespace SmartProduction.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierProduct> SupplierProducts { get; set; }
    public DbSet<BOMItem> BOMItems { get; set; }

    public DbSet<WorkCenter> WorkCenters { get; set; }
    public DbSet<Routing> Routings { get; set; }
    public DbSet<RoutingStep> RoutingSteps { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<MaterialRequirement> MaterialRequirements { get; set; }
    public DbSet<SalesHistory> SalesHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite Key for SupplierProduct
        modelBuilder.Entity<SupplierProduct>()
            .HasKey(sp => new { sp.SupplierId, sp.ProductId });

        // BOM Relations
        modelBuilder.Entity<BOMItem>()
            .HasOne(bi => bi.ParentProduct)
            .WithMany(p => p.ComponentsInBOMs) // Corregido: Nombre real en entidad Product
            .HasForeignKey(bi => bi.ParentProductId)
            .OnDelete(DeleteBehavior.Restrict); // Evitar borrado en cascada

        modelBuilder.Entity<BOMItem>()
            .HasOne(bi => bi.ComponentProduct)
            .WithMany(p => p.ParentInBOMs)
            .HasForeignKey(bi => bi.ComponentProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Routing Relations
        modelBuilder.Entity<Routing>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoutingStep>()
            .HasOne(rs => rs.WorkCenter)
            .WithMany()
            .HasForeignKey(rs => rs.WorkCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkOrder Relations
        modelBuilder.Entity<WorkOrder>()
            .HasOne(wo => wo.Product)
            .WithMany()
            .HasForeignKey(wo => wo.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<WorkOrder>()
            .HasOne(wo => wo.Routing)
            .WithMany()
            .HasForeignKey(wo => wo.RoutingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventory & MRP Relations
        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MaterialRequirement>()
            .HasOne(m => m.Product)
            .WithMany()
            .HasForeignKey(m => m.ProductId)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<SalesHistory>()
            .HasOne(sh => sh.Product)
            .WithMany()
            .HasForeignKey(sh => sh.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Data
        modelBuilder.Entity<UnitOfMeasure>().HasData(
            new UnitOfMeasure { Id = 1, Name = "Unidad", Abbreviation = "UND" },
            new UnitOfMeasure { Id = 2, Name = "Kilogramo", Abbreviation = "KG" },
            new UnitOfMeasure { Id = 3, Name = "Metro", Abbreviation = "M" },
            new UnitOfMeasure { Id = 4, Name = "Litro", Abbreviation = "L" }
        );
        
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 2, Name = "Semielaborado", Description = "Componentes fabricados internamente" },
            new Category { Id = 3, Name = "Producto Terminado", Description = "Productos para venta final" }
        );

        // Seed Sales History (Tendencia creciente para demostración)
        // Asumimos ProductID 1 
        var salesData = new List<SalesHistory>();
        var baseDate = DateTime.Today.AddMonths(-6);
        int shId = 1;
        
        // Simular tendencia lineal: y = 10x + 50 + ruido
        for (int i = 0; i < 6; i++)
        {
            salesData.Add(new SalesHistory 
            { 
                Id = shId++, 
                ProductId = 1, 
                Date = baseDate.AddMonths(i), 
                QuantitySold = 50 + (i * 10) + new Random().Next(-5, 5) 
            });
        }
        
        modelBuilder.Entity<SalesHistory>().HasData(salesData);
    }
}
