using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class DataSeeder
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DataSeeder(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task SeedAsync()
    {
        using var context = _contextFactory.CreateDbContext();

        if (await context.Products.AnyAsync())
        {
            return; // Data already exists
        }

        // 1. Units of Measure
        var units = new List<UnitOfMeasure>
        {
            new() { Name = "Pieza", Abbreviation = "pza" },
            new() { Name = "Metro", Abbreviation = "m" },
            new() { Name = "Kilogramo", Abbreviation = "kg" },
            new() { Name = "Litro", Abbreviation = "l" }
        };
        context.UnitsOfMeasure.AddRange(units);
        await context.SaveChangesAsync();

        var pza = units.First(u => u.Abbreviation == "pza");
        var m = units.First(u => u.Abbreviation == "m");
        var kg = units.First(u => u.Abbreviation == "kg");

        // 2. Categories
        var categories = new List<Category>
        {
            new() { Name = "Electrónica", Description = "Componentes y circuitos" },
            new() { Name = "Mecánica", Description = "Estructuras y tornillería" },
            new() { Name = "Producto Terminado", Description = "Listos para venta" }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var catElec = categories.First(c => c.Name == "Electrónica");
        var catMech = categories.First(c => c.Name == "Mecánica");
        var catFin = categories.First(c => c.Name == "Producto Terminado");

        // 3. Products
        var products = new List<Product>
        {
            // Raw Materials
            new() { SKU = "RM-001", Name = "Resistencia 10k", Description = "Resistencia smd", UnitOfMeasureId = pza.Id, CategoryId = catElec.Id, PurchasePrice = 0.05m, StandardCost = 0.05m },
            new() { SKU = "RM-002", Name = "Capacitor 100uF", Description = "Capacitor electrolítico", UnitOfMeasureId = pza.Id, CategoryId = catElec.Id, PurchasePrice = 0.12m, StandardCost = 0.12m },
            new() { SKU = "RM-003", Name = "Lámina Aluminio", Description = "Lámina 3mm", UnitOfMeasureId = m.Id, CategoryId = catMech.Id, PurchasePrice = 15.00m, StandardCost = 15.00m },
            new() { SKU = "RM-004", Name = "Motor DC", Description = "Motor 12V High Torque", UnitOfMeasureId = pza.Id, CategoryId = catElec.Id, PurchasePrice = 25.00m, StandardCost = 25.00m },
            
            // Subassemblies
            new() { SKU = "SA-001", Name = "PCB Controladora", Description = "Placa principal soldada", UnitOfMeasureId = pza.Id, CategoryId = catElec.Id, IsSubassembly = true, StandardCost = 35.00m },
            new() { SKU = "SA-002", Name = "Chasis Drone", Description = "Estructura base cortada", UnitOfMeasureId = pza.Id, CategoryId = catMech.Id, IsSubassembly = true, StandardCost = 45.00m },

            // Finished Goods
            new() { SKU = "FG-001", Name = "Drone Industrial X1", Description = "Drone para inspección", UnitOfMeasureId = pza.Id, CategoryId = catFin.Id, IsSubassembly = false, StandardCost = 500.00m, CurrentCost = 520.00m },
            new() { SKU = "FG-002", Name = "Sensor IoT Pro", Description = "Sensor ambiental WiFi", UnitOfMeasureId = pza.Id, CategoryId = catFin.Id, IsSubassembly = false, StandardCost = 80.00m, CurrentCost = 85.00m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var pRes = products.First(p => p.SKU == "RM-001");
        var pCap = products.First(p => p.SKU == "RM-002");
        var pLam = products.First(p => p.SKU == "RM-003");
        var pMot = products.First(p => p.SKU == "RM-004");
        var pPcb = products.First(p => p.SKU == "SA-001");
        var pCha = products.First(p => p.SKU == "SA-002");
        var pDrone = products.First(p => p.SKU == "FG-001");
        var pSensor = products.First(p => p.SKU == "FG-002");

        // 4. Work Centers
        var workCenters = new List<WorkCenter>
        {
            new() { Name = "Corte Láser", CostPerHour = 120.00m, CapacityPerDay = 16 },
            new() { Name = "Ensamblaje PCB", CostPerHour = 45.00m, CapacityPerDay = 8 },
            new() { Name = "Ensamblaje Final", CostPerHour = 60.00m, CapacityPerDay = 8 },
            new() { Name = "Testing & QA", CostPerHour = 80.00m, CapacityPerDay = 8 }
        };
        context.WorkCenters.AddRange(workCenters);
        await context.SaveChangesAsync();

        var wcLaser = workCenters.First(wc => wc.Name == "Corte Láser");
        var wcPcb = workCenters.First(wc => wc.Name == "Ensamblaje PCB");
        var wcAssy = workCenters.First(wc => wc.Name == "Ensamblaje Final");
        var wcTest = workCenters.First(wc => wc.Name == "Testing & QA");

        // 5. BOMs (Bill of Materials)
        // Drone needs Chassis + PCB + 4 Motors
        context.BOMItems.Add(new BOMItem { ParentProductId = pDrone.Id, ComponentProductId = pCha.Id, Quantity = 1 });
        context.BOMItems.Add(new BOMItem { ParentProductId = pDrone.Id, ComponentProductId = pPcb.Id, Quantity = 1 });
        context.BOMItems.Add(new BOMItem { ParentProductId = pDrone.Id, ComponentProductId = pMot.Id, Quantity = 4 });

        // Chassis needs metal sheet
        context.BOMItems.Add(new BOMItem { ParentProductId = pCha.Id, ComponentProductId = pLam.Id, Quantity = 0.5m, WastePercentage = 10 }); // 0.5m per chassis

        // PCB needs components
        context.BOMItems.Add(new BOMItem { ParentProductId = pPcb.Id, ComponentProductId = pRes.Id, Quantity = 12 });
        context.BOMItems.Add(new BOMItem { ParentProductId = pPcb.Id, ComponentProductId = pCap.Id, Quantity = 5 });

        await context.SaveChangesAsync();

        // 6. Routings
        // Routing for Drone
        var rDrone = new Routing { Name = "Ensamble Drone Std", ProductId = pDrone.Id, IsActive = true };
        context.Routings.Add(rDrone);
        await context.SaveChangesAsync();
        context.RoutingSteps.AddRange(
            new RoutingStep { RoutingId = rDrone.Id, Sequence = 10, OperationName = "Ensamble Componentes", WorkCenterId = wcAssy.Id, SetupTimeMinutes = 15, RunTimeMinutes = 60 },
            new RoutingStep { RoutingId = rDrone.Id, Sequence = 20, OperationName = "Pruebas de Vuelo", WorkCenterId = wcTest.Id, SetupTimeMinutes = 10, RunTimeMinutes = 30 }
        );

        // 7. Inventory
        // Raw materials stock
        context.InventoryItems.Add(new InventoryItem { ProductId = pRes.Id, QuantityOnHand = 5000, SafetyStock = 1000 });
        context.InventoryItems.Add(new InventoryItem { ProductId = pCap.Id, QuantityOnHand = 2000, SafetyStock = 500 });
        context.InventoryItems.Add(new InventoryItem { ProductId = pLam.Id, QuantityOnHand = 50, SafetyStock = 20 });
        context.InventoryItems.Add(new InventoryItem { ProductId = pMot.Id, QuantityOnHand = 12, SafetyStock = 40 }); // Low stock alert!

        // Finished goods
        context.InventoryItems.Add(new InventoryItem { ProductId = pDrone.Id, QuantityOnHand = 2, SafetyStock = 5 });

        await context.SaveChangesAsync();

        // 8. Work Orders (Current Status)
        context.WorkOrders.AddRange(
            // Completed
            new WorkOrder { OrderNumber = "WO-23-001", ProductId = pDrone.Id, Quantity = 5, Status = WorkOrderStatus.Completed, CreatedDate = DateTime.Today.AddDays(-20), DueDate = DateTime.Today.AddDays(-5), ActualCompletionDate = DateTime.Today.AddDays(-6) },
            // In Progress
            new WorkOrder { OrderNumber = "WO-23-025", ProductId = pDrone.Id, Quantity = 10, Status = WorkOrderStatus.Started, CreatedDate = DateTime.Today.AddDays(-5), DueDate = DateTime.Today.AddDays(10), RoutingId = rDrone.Id },
            // Delayed!
            new WorkOrder { OrderNumber = "WO-23-018", ProductId = pSensor.Id, Quantity = 50, Status = WorkOrderStatus.Released, CreatedDate = DateTime.Today.AddDays(-15), DueDate = DateTime.Today.AddDays(-1) }
        );

        // 9. Suppliers
        context.Suppliers.AddRange(new Supplier { Name = "ElectroParts Global", ContactEmail = "sales@electroparts.com" }, new Supplier { Name = "MetalWorks Inc", ContactEmail = "orders@metalworks.com" });

        // 10. Sales History (For Charts)
        var random = new Random();
        var history = new List<SalesHistory>();
        for (int i = 0; i < 12; i++)
        {
            history.Add(new SalesHistory { ProductId = pDrone.Id, Date = DateTime.Today.AddMonths(-i), QuantitySold = random.Next(5, 25) });
            history.Add(new SalesHistory { ProductId = pSensor.Id, Date = DateTime.Today.AddMonths(-i), QuantitySold = random.Next(20, 80) });
        }
        context.SalesHistory.AddRange(history);

        await context.SaveChangesAsync();
    }
}
